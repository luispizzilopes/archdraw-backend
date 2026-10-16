using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Archdraw.Tests.Integration;

public static class Dsl
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<JsonElement> ReadAsync(this HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();

    public static async Task<Guid> CreateProjectAsync(this HttpClient client, string name = "Projeto")
    {
        var response = await client.PostAsJsonAsync("/api/v1/projects", new { name, description = "desc", primaryStyleId = "microservices" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadAsync()).GetProperty("id").GetGuid();
    }

    public static async Task<Guid> CreateDiagramAsync(this HttpClient client, Guid projectId, string name = "Diagrama", object? snapshot = null)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/diagrams",
            new { name, level = "free", styleIds = new[] { "microservices" }, patternIds = Array.Empty<string>(), snapshot, readme = $"# {name}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadAsync()).GetProperty("id").GetGuid();
    }

    public static object Node(Guid id, string type = "api-rest", string name = "Nó", Guid? componentId = null, Guid? linkedDiagramId = null, double x = 0)
        => new
        {
            id, type, position = new { x, y = 0 }, size = new { width = 200, height = 80 }, zIndex = 1, componentId,
            properties = new { name, linkedDiagramId, metadata = new Dictionary<string, string>() }
        };

    public static object Edge(Guid id, Guid source, Guid target, string kind = "http")
        => new { id, source, target, kind, sync = "sync", direction = "forward", waypoints = new[] { new { x = 10, y = 20 } } };

    public static object Snapshot(long baseRevision, IEnumerable<object> nodes, IEnumerable<object> edges)
        => new
        {
            baseRevision, nodes, edges,
            viewport = new { x = 0, y = 0, zoom = 1 },
            settings = new { showGrid = true, snapToGrid = true, gridSize = 20, showMinimap = true }
        };

    public static async Task<long> RevisionAsync(this HttpClient client, Guid diagramId)
        => (await (await client.GetAsync($"/api/v1/diagrams/{diagramId}")).ReadAsync()).GetProperty("revision").GetInt64();

    public static Task<HttpResponseMessage> SaveAsync(this HttpClient client, Guid diagramId, object snapshot)
        => client.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}/snapshot", snapshot);

    public static async Task<Guid> CreateComponentAsync(this HttpClient client, Guid projectId, string name = "Componente")
    {
        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/components",
            new { typeId = "microservice", properties = new { name, technology = ".NET", responsibility = "Faz coisas" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadAsync()).GetProperty("id").GetGuid();
    }
}
