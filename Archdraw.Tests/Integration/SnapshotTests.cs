using System.Net;
using System.Net.Http.Json;
using static Archdraw.Tests.Integration.Dsl;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public class SnapshotTests(ApiFixture api)
{
    [Fact]
    public async Task Snapshot_persiste_nos_conexoes_waypoints_e_recarrega_identico()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        Guid n1 = Guid.NewGuid(), n2 = Guid.NewGuid(), e1 = Guid.NewGuid();

        var save = await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(n1, name: "API"), Node(n2, "db-relational", "DB", x: 300)], [Edge(e1, n1, n2, "db-access")]));
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        Assert.Equal(2, (await save.ReadAsync()).GetProperty("revision").GetInt64());

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        Assert.Equal(2, snapshot.GetProperty("nodes").GetArrayLength());
        var edge = snapshot.GetProperty("edges")[0];
        Assert.Equal(e1, edge.GetProperty("id").GetGuid());
        Assert.Equal("db-access", edge.GetProperty("kind").GetString());
        Assert.Equal(10, edge.GetProperty("waypoints")[0].GetProperty("x").GetDouble());
        Assert.Contains(snapshot.GetProperty("nodes").EnumerateArray(), n => n.GetProperty("id").GetGuid() == n1 && n.GetProperty("properties").GetProperty("name").GetString() == "API");
    }

    [Fact]
    public async Task Salvar_novamente_remove_o_que_sumiu_e_preserva_ids_estaveis()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        Guid n1 = Guid.NewGuid(), n2 = Guid.NewGuid(), e1 = Guid.NewGuid();
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(n1), Node(n2)], [Edge(e1, n1, n2)]));

        var save = await user.Client.SaveAsync(diagramId, Snapshot(2, [Node(n1, name: "Renomeado")], []));
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        Assert.Single(snapshot.GetProperty("nodes").EnumerateArray());
        Assert.Equal(n1, snapshot.GetProperty("nodes")[0].GetProperty("id").GetGuid());
        Assert.Equal(0, snapshot.GetProperty("edges").GetArrayLength());
        Assert.Equal(3, snapshot.GetProperty("revision").GetInt64());
    }

    [Fact]
    public async Task Revisao_obsoleta_retorna_409_com_a_revisao_atual_sem_sobrescrever()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), name: "Versão A")], []));

        var stale = await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), name: "Versão B")], []));
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var body = await stale.ReadAsync();
        Assert.Equal("Diagram.RevisionConflict", body.GetProperty("code").GetString());
        Assert.Equal(2, body.GetProperty("metadata").GetProperty("currentRevision").GetInt64());

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        Assert.Equal("Versão A", snapshot.GetProperty("nodes")[0].GetProperty("properties").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Salvamentos_concorrentes_com_a_mesma_base_so_um_vence()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        var revision = await user.Client.RevisionAsync(diagramId);

        var tasks = Enumerable.Range(0, 6).Select(i =>
            user.Client.SaveAsync(diagramId, Snapshot(revision, [Node(Guid.NewGuid(), name: $"Salvo {i}")], []))).ToArray();
        var responses = await Task.WhenAll(tasks);

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(5, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(revision + 1, await user.Client.RevisionAsync(diagramId));
    }

    [Fact]
    public async Task Conexao_para_no_inexistente_ou_de_outro_diagrama_e_rejeitada()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var d1 = await user.Client.CreateDiagramAsync(projectId, "D1");
        var d2 = await user.Client.CreateDiagramAsync(projectId, "D2");
        Guid a = Guid.NewGuid(), b = Guid.NewGuid();
        await user.Client.SaveAsync(d2, Snapshot(1, [Node(b)], []));

        var ghost = await user.Client.SaveAsync(d1, Snapshot(1, [Node(a)], [Edge(Guid.NewGuid(), a, Guid.NewGuid())]));
        Assert.Equal(HttpStatusCode.BadRequest, ghost.StatusCode);

        var foreign = await user.Client.SaveAsync(d1, Snapshot(1, [Node(a)], [Edge(Guid.NewGuid(), a, b)]));
        Assert.Equal(HttpStatusCode.BadRequest, foreign.StatusCode);

        Assert.Equal(1, await user.Client.RevisionAsync(d1));
    }

    [Fact]
    public async Task Snapshot_invalido_e_rejeitado_sem_persistir()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        var dup = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.BadRequest, (await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(dup), Node(dup)], []))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), "tipo-inexistente")], []))).StatusCode);

        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(a), Node(b)], [Edge(Guid.NewGuid(), a, b, "kind-inexistente")]))).StatusCode);
        Assert.Equal(1, await user.Client.RevisionAsync(diagramId));
    }

    [Fact]
    public async Task Limite_de_nos_por_snapshot_e_aplicado()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);

        var tooMany = Enumerable.Range(0, 2001).Select(i => Node(Guid.NewGuid(), x: i));
        var response = await user.Client.SaveAsync(diagramId, Snapshot(1, tooMany, []));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Diagrama_aceita_snapshot_inicial_de_template_na_criacao()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        Guid a = Guid.NewGuid(), b = Guid.NewGuid();
        var diagramId = await user.Client.CreateDiagramAsync(projectId, "Com template", Snapshot(0, [Node(a), Node(b)], [Edge(Guid.NewGuid(), a, b)]));

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        Assert.Equal(2, snapshot.GetProperty("nodes").GetArrayLength());
        Assert.Equal(1, snapshot.GetProperty("edges").GetArrayLength());
    }

    [Fact]
    public async Task Vinculo_para_diagrama_de_outro_projeto_e_rejeitado()
    {
        var user = await api.CreateUserAsync();
        var p1 = await user.Client.CreateProjectAsync("P1");
        var p2 = await user.Client.CreateProjectAsync("P2");
        var d1 = await user.Client.CreateDiagramAsync(p1);
        var otherProjectDiagram = await user.Client.CreateDiagramAsync(p2);

        var response = await user.Client.SaveAsync(d1, Snapshot(1, [Node(Guid.NewGuid(), linkedDiagramId: otherProjectDiagram)], []));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
