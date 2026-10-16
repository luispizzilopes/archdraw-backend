using Archdraw.BuildingBlocks.Common.Class.AggregateRoot;
using Archdraw.BuildingBlocks.Common.Interfaces;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Domain.Modules.Diagrams.Aggregates;

public class Diagram : AggregateRoot<Guid>, IAuditableEntity
{
    public const int NameMaxLength = 120;
    public const int DescriptionMaxLength = 2000;
    public const int MaxNodes = 2000;
    public const int MaxConnections = 4000;
    public const int CurrentSchemaVersion = 1;

    private readonly List<DiagramNode> _nodes = [];
    private readonly List<DiagramConnection> _connections = [];

    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Level { get; private set; } = "free";
    public List<string> StyleIds { get; private set; } = [];
    public List<string> PatternIds { get; private set; } = [];
    public Viewport Viewport { get; private set; } = Viewport.Default;
    public DiagramSettings Settings { get; private set; } = DiagramSettings.Default;
    public int SchemaVersion { get; private set; } = CurrentSchemaVersion;
    public long Revision { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DiagramNode> Nodes => _nodes;
    public IReadOnlyCollection<DiagramConnection> Connections => _connections;

    private Diagram() { }

    public static Diagram Create(
        Guid id, Guid projectId, string name, string? description, string level,
        IEnumerable<string>? styleIds, IEnumerable<string>? patternIds)
    {
        if (projectId == Guid.Empty) throw new DomainException("O projeto do diagrama é obrigatório.");
        var diagram = new Diagram { Id = id == Guid.Empty ? Guid.NewGuid() : id, ProjectId = projectId, Revision = 1 };
        diagram.ApplyMetadata(name, description, level, styleIds, patternIds);
        return diagram;
    }

    public void UpdateMetadata(
        string name, string? description, string level, IEnumerable<string>? styleIds, IEnumerable<string>? patternIds)
        => ApplyMetadata(name, description, level, styleIds, patternIds);

    public bool IsAtRevision(long baseRevision) => Revision == baseRevision;

    public void ApplySnapshot(
        IReadOnlyCollection<NodeData> nodes, IReadOnlyCollection<ConnectionData> connections,
        Viewport viewport, DiagramSettings settings)
    {
        if (nodes.Count > MaxNodes) throw new DomainException($"Um diagrama pode ter no máximo {MaxNodes} nós.");
        if (connections.Count > MaxConnections) throw new DomainException($"Um diagrama pode ter no máximo {MaxConnections} conexões.");

        var nodeIds = new HashSet<Guid>();
        foreach (var node in nodes)
            if (!nodeIds.Add(node.Id)) throw new DomainException($"Nó duplicado no snapshot: {node.Id}.");

        var connectionIds = new HashSet<Guid>();
        foreach (var connection in connections)
        {
            if (!connectionIds.Add(connection.Id)) throw new DomainException($"Conexão duplicada no snapshot: {connection.Id}.");
            if (!nodeIds.Contains(connection.SourceNodeId) || !nodeIds.Contains(connection.TargetNodeId))
                throw new DomainException("Uma conexão referencia um nó que não pertence a este diagrama.");
        }

        ValidateViewport(viewport);
        ValidateSettings(settings);

        _connections.RemoveAll(c => !connectionIds.Contains(c.Id));
        _nodes.RemoveAll(n => !nodeIds.Contains(n.Id));

        var existingNodes = _nodes.ToDictionary(n => n.Id);
        foreach (var data in nodes)
        {
            if (existingNodes.TryGetValue(data.Id, out var existing)) existing.Apply(data);
            else _nodes.Add(DiagramNode.Create(Id, data));
        }

        var existingConnections = _connections.ToDictionary(c => c.Id);
        foreach (var data in connections)
        {
            if (existingConnections.TryGetValue(data.Id, out var existing)) existing.Apply(data);
            else _connections.Add(DiagramConnection.Create(Id, data));
        }

        Viewport = viewport;
        Settings = settings;
        SchemaVersion = CurrentSchemaVersion;
        Revision++;
    }

    public Diagram Duplicate(string newName)
    {
        var copy = Create(Guid.NewGuid(), ProjectId, newName, Description, Level, StyleIds, PatternIds);
        var idMap = _nodes.ToDictionary(n => n.Id, _ => Guid.NewGuid());

        var nodeData = _nodes.Select(n => new NodeData(
            idMap[n.Id], n.TypeId, n.X, n.Y, n.Width, n.Height, n.ZIndex,
            n.ComponentId, n.LinkedDiagramId, n.Properties with
            {
                Links = n.Properties.Links.ToList(),
                Metadata = new Dictionary<string, string>(n.Properties.Metadata)
            })).ToList();

        var connData = _connections.Select(c => new ConnectionData(
            Guid.NewGuid(), idMap[c.SourceNodeId], idMap[c.TargetNodeId], c.SourceHandle, c.TargetHandle,
            c.KindId, c.Label, c.Protocol, c.Description, c.Sync, c.Direction, c.Waypoints.ToList())).ToList();

        copy.ApplySnapshot(nodeData, connData, Viewport, Settings);
        copy.Revision = 1;
        return copy;
    }

    private void ApplyMetadata(
        string name, string? description, string level, IEnumerable<string>? styleIds, IEnumerable<string>? patternIds)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length == 0) throw new DomainException("O nome do diagrama é obrigatório.");
        if (name.Length > NameMaxLength) throw new DomainException($"O nome do diagrama deve ter no máximo {NameMaxLength} caracteres.");
        description = (description ?? string.Empty).Trim();
        if (description.Length > DescriptionMaxLength) throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");
        if (!ArchitecturalCatalog.DiagramLevels.Contains(level)) throw new DomainException($"Nível de diagrama inválido: {level}.");

        var styles = (styleIds ?? []).Distinct().ToList();
        var patterns = (patternIds ?? []).Distinct().ToList();
        if (styles.Any(s => !ArchitecturalCatalog.IsStyle(s))) throw new DomainException("Estilo arquitetural desconhecido.");
        if (patterns.Any(p => !ArchitecturalCatalog.IsPattern(p))) throw new DomainException("Padrão arquitetural desconhecido.");

        Name = name;
        Description = description;
        Level = level;
        StyleIds = styles;
        PatternIds = patterns;
    }

    private static void ValidateViewport(Viewport v)
    {
        if (!double.IsFinite(v.X) || !double.IsFinite(v.Y) || !double.IsFinite(v.Zoom) || v.Zoom <= 0 || v.Zoom > 20)
            throw new DomainException("Viewport inválido.");
    }

    private static void ValidateSettings(DiagramSettings s)
    {
        if (s.GridSize is < 4 or > 200) throw new DomainException("Tamanho de grade inválido.");
    }

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
