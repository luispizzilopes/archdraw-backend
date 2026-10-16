namespace Archdraw.Application.Modules.Diagrams.Requests;

public sealed record PointDto(double X, double Y);
public sealed record SizeDto(double Width, double Height);
public sealed record ViewportDto(double X, double Y, double Zoom);
public sealed record DiagramSettingsDto(bool ShowGrid, bool SnapToGrid, int GridSize, bool ShowMinimap);
public sealed record ComponentLinkDto(string Label, string Url);

public sealed record NodePropertiesDto(
    string Name, string? Identifier, string? Description, string? Technology, string? Responsibility,
    string? Notes, string? PatternId, List<ComponentLinkDto>? Links, Guid? LinkedDiagramId,
    Dictionary<string, string>? Metadata);

public sealed record NodeDto(
    Guid Id, string Type, PointDto Position, SizeDto Size, int ZIndex, Guid? ComponentId, NodePropertiesDto Properties);

public sealed record EdgeDto(
    Guid Id, Guid Source, Guid Target, string? SourceHandle, string? TargetHandle, string Kind,
    string? Label, string? Protocol, string? Description, string Sync, string Direction, List<PointDto>? Waypoints);

public sealed record SnapshotDto(
    List<NodeDto> Nodes, List<EdgeDto> Edges, ViewportDto Viewport, DiagramSettingsDto Settings);

public sealed record CreateDiagramRequest(
    Guid? Id, string Name, string? Description, string Level, List<string>? StyleIds, List<string>? PatternIds,
    SnapshotDto? Snapshot, string? Readme);

public sealed record UpdateDiagramRequest(
    string Name, string? Description, string Level, List<string>? StyleIds, List<string>? PatternIds);

public sealed record DuplicateDiagramRequest(string? Name);

public sealed record SaveSnapshotRequest(
    long BaseRevision, List<NodeDto> Nodes, List<EdgeDto> Edges, ViewportDto Viewport, DiagramSettingsDto Settings);
