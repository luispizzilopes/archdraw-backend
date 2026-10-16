using Archdraw.Application.Modules.Diagrams.Requests;

namespace Archdraw.Application.Modules.Diagrams.Responses;

public sealed record DiagramResponse(
    Guid Id, Guid ProjectId, string Name, string Description, string Level, List<string> StyleIds,
    List<string> PatternIds, long Revision, int NodeCount, int ConnectionCount,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed record DiagramSnapshotResponse(
    Guid DiagramId, long Revision, int SchemaVersion, List<NodeDto> Nodes, List<EdgeDto> Edges,
    ViewportDto Viewport, DiagramSettingsDto Settings, DateTime UpdatedAtUtc);

public sealed record SaveSnapshotResponse(long Revision, DateTime UpdatedAtUtc);
