using Archdraw.BuildingBlocks.Common.Class.Base;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Domain.Modules.Diagrams.Entities;

public class DiagramConnection : Entity<Guid>
{
    public const int MaxWaypoints = 50;

    public Guid DiagramId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public string? SourceHandle { get; private set; }
    public string? TargetHandle { get; private set; }
    public string KindId { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Protocol { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sync { get; private set; } = "sync";
    public string Direction { get; private set; } = "forward";
    public List<Point> Waypoints { get; private set; } = [];

    private DiagramConnection() { }

    internal static DiagramConnection Create(Guid diagramId, ConnectionData data)
    {
        var connection = new DiagramConnection { Id = data.Id, DiagramId = diagramId };
        connection.Apply(data);
        return connection;
    }

    internal void Apply(ConnectionData data)
    {
        if (data.Id == Guid.Empty) throw new DomainException("Toda conexão precisa de um identificador.");
        if (!ArchitecturalCatalog.IsConnectionKind(data.KindId))
            throw new DomainException($"Tipo de conexão desconhecido: {data.KindId}.");
        if (data.Sync is not ("sync" or "async")) throw new DomainException("Sincronia da conexão inválida.");
        if (data.Direction is not ("forward" or "both" or "none")) throw new DomainException("Direção da conexão inválida.");
        var waypoints = data.Waypoints ?? [];
        if (waypoints.Count > MaxWaypoints) throw new DomainException($"No máximo {MaxWaypoints} pontos de passagem por conexão.");
        if (waypoints.Any(p => !double.IsFinite(p.X) || !double.IsFinite(p.Y)))
            throw new DomainException("Pontos de passagem inválidos.");

        SourceNodeId = data.SourceNodeId;
        TargetNodeId = data.TargetNodeId;
        SourceHandle = data.SourceHandle;
        TargetHandle = data.TargetHandle;
        KindId = data.KindId;
        Label = data.Label ?? string.Empty;
        Protocol = data.Protocol ?? string.Empty;
        Description = data.Description ?? string.Empty;
        Sync = data.Sync;
        Direction = data.Direction;
        Waypoints = waypoints;
    }
}

public sealed record ConnectionData(
    Guid Id, Guid SourceNodeId, Guid TargetNodeId, string? SourceHandle, string? TargetHandle,
    string KindId, string Label, string Protocol, string Description, string Sync, string Direction,
    List<Point>? Waypoints);
