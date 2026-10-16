using System.Net;
using System.Net.Http.Json;
using static Archdraw.Tests.Integration.Dsl;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ComponentsAndDocumentsTests(ApiFixture api)
{
    [Fact]
    public async Task Componente_e_reutilizado_em_varios_diagramas_e_edicao_reflete_em_todos()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var componentId = await user.Client.CreateComponentAsync(projectId, "Pagamentos");
        var d1 = await user.Client.CreateDiagramAsync(projectId, "D1");
        var d2 = await user.Client.CreateDiagramAsync(projectId, "D2");
        await user.Client.SaveAsync(d1, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "ignorado", componentId)], []));
        await user.Client.SaveAsync(d2, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "ignorado", componentId)], []));

        var update = await user.Client.PutAsJsonAsync($"/api/v1/components/{componentId}",
            new { typeId = "microservice", properties = new { name = "Pagamentos v2", technology = "Go" } });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(2, (await update.ReadAsync()).GetProperty("usageCount").GetInt32());

        foreach (var d in new[] { d1, d2 })
        {
            var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{d}/snapshot")).ReadAsync();
            var props = snapshot.GetProperty("nodes")[0].GetProperty("properties");
            Assert.Equal("Pagamentos v2", props.GetProperty("name").GetString());
            Assert.Equal("Go", props.GetProperty("technology").GetString());
            Assert.Equal(componentId, snapshot.GetProperty("nodes")[0].GetProperty("componentId").GetGuid());
        }
    }

    [Fact]
    public async Task Salvar_snapshot_nao_sobrescreve_o_componente_com_propriedades_obsoletas_do_no()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var componentId = await user.Client.CreateComponentAsync(projectId, "Original");
        var d1 = await user.Client.CreateDiagramAsync(projectId);
        await user.Client.PutAsJsonAsync($"/api/v1/components/{componentId}", new { typeId = "microservice", properties = new { name = "Atualizado" } });

        await user.Client.SaveAsync(d1, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "Original (obsoleto)", componentId)], []));

        var component = await (await user.Client.GetAsync($"/api/v1/components/{componentId}")).ReadAsync();
        Assert.Equal("Atualizado", component.GetProperty("properties").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Excluir_componente_em_uso_exige_confirmacao_e_desvincula_os_nos()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var componentId = await user.Client.CreateComponentAsync(projectId, "Em uso");
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "x", componentId)], []));

        var blocked = await user.Client.DeleteAsync($"/api/v1/components/{componentId}");
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Equal(1, (await blocked.ReadAsync()).GetProperty("metadata").GetProperty("usageCount").GetInt32());

        var detached = await user.Client.DeleteAsync($"/api/v1/components/{componentId}?detach=true");
        Assert.Equal(HttpStatusCode.NoContent, detached.StatusCode);

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        var node = snapshot.GetProperty("nodes")[0];
        Assert.Equal(System.Text.Json.JsonValueKind.Null, node.GetProperty("componentId").ValueKind);
        Assert.Equal("Em uso", node.GetProperty("properties").GetProperty("name").GetString());
    }

    [Fact]
    public async Task No_nao_pode_referenciar_componente_de_outro_projeto()
    {
        var user = await api.CreateUserAsync();
        var p1 = await user.Client.CreateProjectAsync("P1");
        var p2 = await user.Client.CreateProjectAsync("P2");
        var foreignComponent = await user.Client.CreateComponentAsync(p2);
        var diagramId = await user.Client.CreateDiagramAsync(p1);

        var response = await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "x", foreignComponent)], []));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Listagem_de_componentes_traz_contagem_de_uso()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var used = await user.Client.CreateComponentAsync(projectId, "Usado");
        await user.Client.CreateComponentAsync(projectId, "Livre");
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), "microservice", "x", used), Node(Guid.NewGuid(), "microservice", "y", used)], []));

        var list = (await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/components")).ReadAsync()).EnumerateArray().ToList();
        Assert.Equal(2, list.First(c => c.GetProperty("properties").GetProperty("name").GetString() == "Usado").GetProperty("usageCount").GetInt32());
        Assert.Equal(0, list.First(c => c.GetProperty("properties").GetProperty("name").GetString() == "Livre").GetProperty("usageCount").GetInt32());
    }

    [Fact]
    public async Task Documentacao_e_criada_atualizada_e_protegida_contra_sobrescrita()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();

        var empty = await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/documentation")).ReadAsync();
        Assert.Equal(0, empty.GetProperty("revision").GetInt32());

        var first = await user.Client.PutAsJsonAsync($"/api/v1/projects/{projectId}/documentation", new { markdown = "# v1", baseRevision = 0 });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(1, (await first.ReadAsync()).GetProperty("revision").GetInt32());

        var second = await user.Client.PutAsJsonAsync($"/api/v1/projects/{projectId}/documentation", new { markdown = "# v2", baseRevision = 1 });
        Assert.Equal(2, (await second.ReadAsync()).GetProperty("revision").GetInt32());

        var stale = await user.Client.PutAsJsonAsync($"/api/v1/projects/{projectId}/documentation", new { markdown = "# velho", baseRevision = 1 });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        var current = await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/documentation")).ReadAsync();
        Assert.Equal("# v2", current.GetProperty("markdown").GetString());
    }

    [Fact]
    public async Task Documentacao_permanece_vinculada_apos_renomear_e_limite_de_tamanho_vale()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId, "Antes");
        await user.Client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}", new { name = "Depois", level = "free" });

        var doc = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/documentation")).ReadAsync();
        Assert.Equal("# Antes", doc.GetProperty("markdown").GetString());

        var tooBig = await user.Client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}/documentation",
            new { markdown = new string('x', 200_001), baseRevision = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, tooBig.StatusCode);
    }
}
