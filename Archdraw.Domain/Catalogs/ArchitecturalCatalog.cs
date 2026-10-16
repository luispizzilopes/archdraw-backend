namespace Archdraw.Domain.Catalogs;

public static class ArchitecturalCatalog
{
    public static readonly IReadOnlyList<CatalogItem> Styles =
    [
        new("monolith", "Monolithic Architecture"),
        new("modular-monolith", "Modular Monolith"),
        new("microservices", "Microservices Architecture"),
        new("event-driven", "Event-Driven Architecture"),
    ];

    public static readonly IReadOnlyList<CatalogItem> Patterns =
    [
        new("clean-architecture", "Clean Architecture"),
        new("vertical-slice", "Vertical Slice Architecture"),
        new("layered", "Layered Architecture"),
    ];

    public static readonly IReadOnlyList<CatalogItem> ComponentTypes = new[]
    {
        "api-rest", "api-gateway", "microservice", "monolith", "module", "frontend-app", "bff", "worker",
        "background-job", "external-service", "db-relational", "db-nosql", "cache", "object-storage",
        "data-warehouse", "message-broker", "queue", "topic", "event-bus", "producer", "consumer", "webhook",
        "load-balancer", "container", "k8s-cluster", "serverless", "cdn", "reverse-proxy", "domain",
        "application", "infrastructure", "presentation", "controller", "use-case", "handler", "repository",
        "entity", "aggregate", "domain-event", "feature", "command", "query", "validator", "person",
        "software-system", "boundary", "note",
    }.Select(id => new CatalogItem(id, id)).ToList();

    public static readonly IReadOnlyList<CatalogItem> ConnectionKinds = new[]
    {
        "http", "grpc", "event-publish", "event-subscribe", "queue-producer", "queue-consumer",
        "db-access", "dependency", "internal",
    }.Select(id => new CatalogItem(id, id)).ToList();

    public static readonly IReadOnlySet<string> DiagramLevels =
        new HashSet<string> { "free", "context", "container", "component" };

    private static readonly HashSet<string> StyleIds = Styles.Select(s => s.Id).ToHashSet();
    private static readonly HashSet<string> PatternIds = Patterns.Select(s => s.Id).ToHashSet();
    private static readonly HashSet<string> ComponentTypeIds = ComponentTypes.Select(s => s.Id).ToHashSet();
    private static readonly HashSet<string> ConnectionKindIds = ConnectionKinds.Select(s => s.Id).ToHashSet();

    public static bool IsStyle(string? id) => id is not null && StyleIds.Contains(id);
    public static bool IsPattern(string? id) => id is not null && PatternIds.Contains(id);
    public static bool IsComponentType(string? id) => id is not null && ComponentTypeIds.Contains(id);
    public static bool IsConnectionKind(string? id) => id is not null && ConnectionKindIds.Contains(id);
}

public sealed record CatalogItem(string Id, string Name);
