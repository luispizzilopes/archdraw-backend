using System.Net;
using System.Net.Http.Json;
using static Archdraw.Tests.Integration.Dsl;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public class DiagramLifecycleTests(ApiFixture api)
{
    [Fact]
    public async Task Projeto_lista_diagramas_e_contagem()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        await user.Client.CreateDiagramAsync(projectId, "A");
        await user.Client.CreateDiagramAsync(projectId, "B");

        var list = await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/diagrams")).ReadAsync();
        Assert.Equal(2, list.GetArrayLength());
        var project = await (await user.Client.GetAsync($"/api/v1/projects/{projectId}")).ReadAsync();
        Assert.Equal(2, project.GetProperty("diagramCount").GetInt32());
    }

    [Fact]
    public async Task Duplicar_cria_copia_independente_com_novos_ids_e_readme()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId, "Original");
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), e = Guid.NewGuid();
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(a), Node(b)], [Edge(e, a, b)]));

        var response = await user.Client.PostAsJsonAsync($"/api/v1/diagrams/{diagramId}/duplicate", new { name = "Cópia" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var copyId = (await response.ReadAsync()).GetProperty("id").GetGuid();
        Assert.NotEqual(diagramId, copyId);

        var original = await (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).ReadAsync();
        var copy = await (await user.Client.GetAsync($"/api/v1/diagrams/{copyId}/snapshot")).ReadAsync();
        Assert.Equal(2, copy.GetProperty("nodes").GetArrayLength());
        Assert.Equal(1, copy.GetProperty("edges").GetArrayLength());

        var originalIds = original.GetProperty("nodes").EnumerateArray().Select(n => n.GetProperty("id").GetGuid()).ToHashSet();
        Assert.DoesNotContain(copy.GetProperty("nodes").EnumerateArray(), n => originalIds.Contains(n.GetProperty("id").GetGuid()));
        var copyNodeIds = copy.GetProperty("nodes").EnumerateArray().Select(n => n.GetProperty("id").GetGuid()).ToHashSet();
        var copyEdge = copy.GetProperty("edges")[0];
        Assert.Contains(copyEdge.GetProperty("source").GetGuid(), copyNodeIds);
        Assert.Contains(copyEdge.GetProperty("target").GetGuid(), copyNodeIds);

        var readme = await (await user.Client.GetAsync($"/api/v1/diagrams/{copyId}/documentation")).ReadAsync();
        Assert.Equal("# Original", readme.GetProperty("markdown").GetString());
    }

    [Fact]
    public async Task Excluir_diagrama_limpa_vinculos_remove_readme_e_preserva_componentes_compartilhados()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var componentId = await user.Client.CreateComponentAsync(projectId, "Compartilhado");
        var detail = await user.Client.CreateDiagramAsync(projectId, "Detalhe");
        var d1 = await user.Client.CreateDiagramAsync(projectId, "D1");
        var d2 = await user.Client.CreateDiagramAsync(projectId, "D2");
        Guid n1 = Guid.NewGuid(), n2 = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.OK, (await user.Client.SaveAsync(d1, Snapshot(1, [Node(n1, componentId: componentId, linkedDiagramId: detail)], []))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await user.Client.SaveAsync(d2, Snapshot(1, [Node(n2, componentId: componentId)], []))).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await user.Client.DeleteAsync($"/api/v1/diagrams/{d2}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await user.Client.GetAsync($"/api/v1/diagrams/{d2}")).StatusCode);
        var component = await (await user.Client.GetAsync($"/api/v1/components/{componentId}")).ReadAsync();
        Assert.Equal(1, component.GetProperty("usageCount").GetInt32());

        Assert.Equal(HttpStatusCode.NoContent, (await user.Client.DeleteAsync($"/api/v1/diagrams/{detail}")).StatusCode);
        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{d1}/snapshot")).ReadAsync();
        Assert.Equal(JsonValueKind(null), snapshot.GetProperty("nodes")[0].GetProperty("properties").GetProperty("linkedDiagramId").ValueKind);
    }

    private static System.Text.Json.JsonValueKind JsonValueKind(object? _) => System.Text.Json.JsonValueKind.Null;

    [Fact]
    public async Task Excluir_projeto_remove_tudo_em_cascata_e_nao_afeta_outros()
    {
        var user = await api.CreateUserAsync();
        var keep = await user.Client.CreateProjectAsync("Fica");
        var keepDiagram = await user.Client.CreateDiagramAsync(keep);
        var projectId = await user.Client.CreateProjectAsync("Some");
        var componentId = await user.Client.CreateComponentAsync(projectId);
        var diagramId = await user.Client.CreateDiagramAsync(projectId);
        await user.Client.SaveAsync(diagramId, Snapshot(1, [Node(Guid.NewGuid(), componentId: componentId)], []));

        Assert.Equal(HttpStatusCode.NoContent, (await user.Client.DeleteAsync($"/api/v1/projects/{projectId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await user.Client.GetAsync($"/api/v1/diagrams/{diagramId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await user.Client.GetAsync($"/api/v1/components/{componentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await user.Client.GetAsync($"/api/v1/diagrams/{keepDiagram}")).StatusCode);
    }

    [Fact]
    public async Task Metadados_do_diagrama_sao_validados_pelo_dominio()
    {
        var user = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var diagramId = await user.Client.CreateDiagramAsync(projectId);

        var badLevel = await user.Client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}", new { name = "X", level = "inexistente" });
        Assert.Equal(HttpStatusCode.BadRequest, badLevel.StatusCode);
        var badStyle = await user.Client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}", new { name = "X", level = "free", styleIds = new[] { "nao-existe" } });
        Assert.Equal(HttpStatusCode.BadRequest, badStyle.StatusCode);

        var ok = await user.Client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}", new { name = "Renomeado", level = "container", styleIds = new[] { "microservices", "event-driven" }, patternIds = new[] { "clean-architecture" } });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal("Renomeado", (await ok.ReadAsync()).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Estilos_e_padroes_coexistem_no_projeto()
    {
        var user = await api.CreateUserAsync();
        var response = await user.Client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Híbrido", primaryStyleId = "modular-monolith", secondaryStyleIds = new[] { "event-driven" }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadAsync();
        Assert.Equal("modular-monolith", body.GetProperty("primaryStyleId").GetString());
        Assert.Equal("event-driven", body.GetProperty("secondaryStyleIds")[0].GetString());

        var invalid = await user.Client.PostAsJsonAsync("/api/v1/projects", new { name = "Ruim", primaryStyleId = "nao-existe" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Catalogos_sao_publicos_e_tipados()
    {
        var catalogs = await (await api.CreateClient().GetAsync("/api/v1/catalogs")).ReadAsync();
        Assert.Equal(4, catalogs.GetProperty("styles").GetArrayLength());
        Assert.Equal(3, catalogs.GetProperty("patterns").GetArrayLength());
        Assert.True(catalogs.GetProperty("componentTypes").GetArrayLength() >= 40);
        Assert.Equal(9, catalogs.GetProperty("connectionKinds").GetArrayLength());
    }

    [Fact]
    public async Task Recentes_e_pais_refletem_edicao_e_vinculos_apenas_do_proprio_usuario()
    {
        var user = await api.CreateUserAsync();
        var other = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        var child = await user.Client.CreateDiagramAsync(projectId, "Filho");
        var parent = await user.Client.CreateDiagramAsync(projectId, "Pai");
        await user.Client.SaveAsync(parent, Snapshot(1, [Node(Guid.NewGuid(), linkedDiagramId: child)], []));
        await other.Client.CreateDiagramAsync(await other.Client.CreateProjectAsync(), "Alheio");

        var recent = (await (await user.Client.GetAsync("/api/v1/diagrams/recent?limit=5")).ReadAsync()).EnumerateArray().ToList();
        Assert.Equal(["Pai", "Filho"], recent.Select(d => d.GetProperty("name").GetString()));

        var parents = (await (await user.Client.GetAsync($"/api/v1/diagrams/{child}/parents")).ReadAsync()).EnumerateArray().ToList();
        Assert.Equal("Pai", Assert.Single(parents).GetProperty("name").GetString());
        Assert.Equal(HttpStatusCode.NotFound, (await other.Client.GetAsync($"/api/v1/diagrams/{child}/parents")).StatusCode);
    }

    [Fact]
    public async Task Resumo_do_dashboard_conta_apenas_dados_do_usuario()
    {
        var user = await api.CreateUserAsync();
        var other = await api.CreateUserAsync();
        var projectId = await user.Client.CreateProjectAsync();
        await user.Client.CreateDiagramAsync(projectId, "A");
        await user.Client.CreateDiagramAsync(projectId, "B");
        await user.Client.CreateComponentAsync(projectId);
        await other.Client.CreateDiagramAsync(await other.Client.CreateProjectAsync(), "Alheio");

        var summary = await (await user.Client.GetAsync("/api/v1/projects/summary")).ReadAsync();
        Assert.Equal(1, summary.GetProperty("projects").GetInt32());
        Assert.Equal(2, summary.GetProperty("diagrams").GetInt32());
        Assert.Equal(1, summary.GetProperty("components").GetInt32());
        Assert.Equal(2, summary.GetProperty("documents").GetInt32());
    }
}
