namespace Archdraw.Domain.Catalogs;

public static class ArchitecturalCatalog
{
    public static readonly IReadOnlyList<CatalogItem> Styles =
    [
        new("monolith", "Monolithic Architecture"),
        new("modular-monolith", "Modular Monolith"),
        new("microservices", "Microservices Architecture"),
        new("event-driven", "Event-Driven Architecture"),
        new("soa", "Service-Oriented Architecture (SOA)"),
        new("service-based", "Service-Based Architecture"),
        new("serverless", "Serverless Architecture"),
        new("microkernel", "Microkernel (Plug-in) Architecture"),
        new("space-based", "Space-Based Architecture"),
        new("pipe-and-filter", "Pipe and Filter Architecture"),
        new("client-server", "Client-Server Architecture"),
        new("peer-to-peer", "Peer-to-Peer Architecture"),
    ];

    public static readonly IReadOnlyList<CatalogItem> Patterns =
    [
        new("clean-architecture", "Clean Architecture"),
        new("vertical-slice", "Vertical Slice Architecture"),
        new("layered", "Layered Architecture"),
        new("hexagonal", "Hexagonal Architecture (Ports & Adapters)"),
        new("onion", "Onion Architecture"),
        new("ddd", "Domain-Driven Design (DDD)"),
        new("cqrs", "CQRS"),
        new("event-sourcing", "Event Sourcing"),
        new("saga", "Saga"),
        new("outbox", "Transactional Outbox"),
        new("mvc", "MVC"),
        new("mvvm", "MVVM"),
        new("publish-subscribe", "Publish-Subscribe"),
        new("strangler-fig", "Strangler Fig"),
        new("anti-corruption-layer", "Anti-Corruption Layer"),
        new("database-per-service", "Database per Service"),
        new("api-composition", "API Composition"),
        new("circuit-breaker", "Circuit Breaker"),
        new("bulkhead", "Bulkhead"),
        new("sidecar", "Sidecar"),
        new("cache-aside", "Cache-Aside"),
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
        "aggregate-root", "value-object", "domain-service", "application-service", "integration-event",
        "event-handler", "dto", "mapper", "port", "adapter",
        "anti-corruption-layer", "unit-of-work", "read-model", "event-store", "saga",
        "outbox", "policy", "middleware", "bounded-context", "dp-factory-method",
        "dp-abstract-factory", "dp-builder", "dp-prototype", "dp-singleton", "dp-adapter",
        "dp-bridge", "dp-composite", "dp-decorator", "dp-facade", "dp-flyweight",
        "dp-proxy", "dp-chain-of-responsibility", "dp-command", "dp-iterator", "dp-mediator",
        "dp-memento", "dp-observer", "dp-state", "dp-strategy", "dp-template-method",
        "dp-visitor", "dp-specification", "dp-dependency-injection", "dp-null-object",
        "lang-class", "lang-abstract-class", "lang-interface", "lang-struct", "lang-record",
        "lang-enum", "lang-union-type", "lang-tuple", "lang-generic", "lang-static-class",
        "lang-trait", "lang-function", "lang-method", "lang-constructor", "lang-lambda",
        "lang-delegate", "lang-language-event", "lang-property", "lang-field", "lang-constant",
        "lang-variable", "lang-namespace", "lang-annotation", "lang-exception", "lang-extension-method",
        "lang-async", "lang-iterator",
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
