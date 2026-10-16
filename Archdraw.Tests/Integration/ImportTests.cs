using System.Net;
using System.Net.Http.Json;
using static Archdraw.Tests.Integration.Dsl;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ImportTests(ApiFixture api)
{
    private static object Bundle(string projectId = "legacy-project-1") => new
    {
        format = "archdraw.project", version = 1, asCopy = false,
        project = new
        {
            id = projectId, name = "Legado", description = "vindo do IndexedDB", primaryStyleId = "microservices", secondaryStyleIds = new[] { "event-driven" },
            reusableComponents = new[]
            {
                new { id = "rc-1", typeId = "microservice", size = new { width = 200, height = 80 },
                      properties = new { name = "Reutilizável", technology = ".NET", links = Array.Empty<object>(), metadata = new Dictionary<string, string>() } }
            }
        },
        diagrams = new object[]
        {
            new
            {
                id = "dg-1", name = "Contexto", description = "", level = "context", styleIds = new[] { "microservices" }, patternIds = Array.Empty<string>(),
                nodes = new object[]
                {
                    new { id = "n-1", type = "api-rest", position = new { x = 0, y = 0 }, size = new { width = 200, height = 80 }, zIndex = 1,
                          properties = new { name = "API", linkedDiagramId = "dg-2", metadata = new Dictionary<string, string>() } },
                    new { id = "n-2", type = "db-relational", position = new { x = 300, y = 0 }, size = new { width = 200, height = 80 }, zIndex = 2,
                          properties = new { name = "DB", metadata = new Dictionary<string, string>() }, componentId = "rc-1" },
                },
                edges = new object[]
                {
                    new { id = "e-1", source = "n-1", target = "n-2", kind = "db-access", label = "SQL", sync = "sync", direction = "forward", waypoints = new[] { new { x = 5, y = 5 } } },
                    new { id = "e-orfa", source = "n-1", target = "n-inexistente", kind = "http", sync = "sync", direction = "forward" },
                },
                viewport = new { x = 10, y = 20, zoom = 0.8 },
                settings = new { showGrid = true, snapToGrid = false, gridSize = 20, showMinimap = true }
            },
            new { id = "dg-2", name = "Detalhe", description = "", level = "container", styleIds = Array.Empty<string>(), patternIds = Array.Empty<string>(),
                  nodes = Array.Empty<object>(), edges = Array.Empty<object>() }
        },
        documents = new object[]
        {
            new { scope = "project", ownerId = projectId, content = "# Legado" },
            new { scope = "diagram", ownerId = "dg-1", content = "# Contexto" },
            new { scope = "diagram", ownerId = "dg-fantasma", content = "# Sem dono" },
        }
    };

    [Fact]
    public async Task Importacao_preserva_diagramas_vinculos_documentos_e_componentes_e_informa_avisos()
    {
        var user = await api.CreateUserAsync();
        var response = await user.Client.PostAsJsonAsync("/api/v1/projects/import", Bundle());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.ReadAsync();

        Assert.False(result.GetProperty("alreadyImported").GetBoolean());
        Assert.Equal(2, result.GetProperty("diagrams").GetInt32());
        Assert.Equal(2, result.GetProperty("nodes").GetInt32());
        Assert.Equal(1, result.GetProperty("connections").GetInt32());
        Assert.Equal(1, result.GetProperty("components").GetInt32());
        Assert.Equal(2, result.GetProperty("documents").GetInt32());
        Assert.True(result.GetProperty("warnings").GetArrayLength() >= 2);

        var projectId = result.GetProperty("projectId").GetGuid();
        var diagrams = (await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/diagrams")).ReadAsync()).EnumerateArray().ToList();
        var context = diagrams.First(d => d.GetProperty("name").GetString() == "Contexto");
        var detail = diagrams.First(d => d.GetProperty("name").GetString() == "Detalhe");

        var snapshot = await (await user.Client.GetAsync($"/api/v1/diagrams/{context.GetProperty("id").GetGuid()}/snapshot")).ReadAsync();
        var api1 = snapshot.GetProperty("nodes").EnumerateArray().First(n => n.GetProperty("properties").GetProperty("name").GetString() == "API");
        Assert.Equal(detail.GetProperty("id").GetGuid(), api1.GetProperty("properties").GetProperty("linkedDiagramId").GetGuid());
        Assert.Equal(0.8, snapshot.GetProperty("viewport").GetProperty("zoom").GetDouble());
        Assert.Equal(5, snapshot.GetProperty("edges")[0].GetProperty("waypoints")[0].GetProperty("x").GetDouble());

        var linkedNode = snapshot.GetProperty("nodes").EnumerateArray().First(n => n.GetProperty("properties").GetProperty("name").GetString() != "API");
        Assert.NotEqual(System.Text.Json.JsonValueKind.Null, linkedNode.GetProperty("componentId").ValueKind);

        var readme = await (await user.Client.GetAsync($"/api/v1/diagrams/{context.GetProperty("id").GetGuid()}/documentation")).ReadAsync();
        Assert.Equal("# Contexto", readme.GetProperty("markdown").GetString());
        var projectDoc = await (await user.Client.GetAsync($"/api/v1/projects/{projectId}/documentation")).ReadAsync();
        Assert.Equal("# Legado", projectDoc.GetProperty("markdown").GetString());
    }

    [Fact]
    public async Task Reimportar_a_migracao_e_idempotente_e_nao_duplica()
    {
        var user = await api.CreateUserAsync();
        var first = await (await user.Client.PostAsJsonAsync("/api/v1/projects/import", Bundle("legacy-idem"))).ReadAsync();
        var second = await (await user.Client.PostAsJsonAsync("/api/v1/projects/import", Bundle("legacy-idem"))).ReadAsync();

        Assert.False(first.GetProperty("alreadyImported").GetBoolean());
        Assert.True(second.GetProperty("alreadyImported").GetBoolean());
        Assert.Equal(first.GetProperty("projectId").GetGuid(), second.GetProperty("projectId").GetGuid());

        var list = await (await user.Client.GetAsync("/api/v1/projects")).ReadAsync();
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Mesmos_dados_locais_em_contas_diferentes_nao_colidem()
    {
        var alice = await api.CreateUserAsync();
        var bob = await api.CreateUserAsync();
        var a = await (await alice.Client.PostAsJsonAsync("/api/v1/projects/import", Bundle("legacy-shared"))).ReadAsync();
        var b = await (await bob.Client.PostAsJsonAsync("/api/v1/projects/import", Bundle("legacy-shared"))).ReadAsync();

        Assert.False(b.GetProperty("alreadyImported").GetBoolean());
        Assert.NotEqual(a.GetProperty("projectId").GetGuid(), b.GetProperty("projectId").GetGuid());
    }

    [Fact]
    public async Task Importar_como_copia_gera_sempre_um_projeto_novo()
    {
        var user = await api.CreateUserAsync();
        var payload = Bundle("legacy-copy");
        var copyBundle = System.Text.Json.JsonSerializer.SerializeToNode(payload, Json)!;
        copyBundle["asCopy"] = true;

        var one = await (await user.Client.PostAsJsonAsync("/api/v1/projects/import", copyBundle)).ReadAsync();
        var two = await (await user.Client.PostAsJsonAsync("/api/v1/projects/import", copyBundle)).ReadAsync();
        Assert.NotEqual(one.GetProperty("projectId").GetGuid(), two.GetProperty("projectId").GetGuid());
    }

    [Fact]
    public async Task Arquivo_com_formato_desconhecido_e_rejeitado()
    {
        var user = await api.CreateUserAsync();
        var response = await user.Client.PostAsJsonAsync("/api/v1/projects/import",
            new { format = "outro", version = 1, project = new { id = "x", name = "x" } });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
