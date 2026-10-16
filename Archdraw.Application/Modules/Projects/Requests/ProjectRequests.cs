namespace Archdraw.Application.Modules.Projects.Requests;

public sealed record CreateProjectRequest(
    Guid? Id, string Name, string? Description, string? PrimaryStyleId, List<string>? SecondaryStyleIds);

public sealed record UpdateProjectRequest(
    string Name, string? Description, string? PrimaryStyleId, List<string>? SecondaryStyleIds);

public sealed record ImportProjectRequest(
    string Format, int Version, LegacyProject Project, List<LegacyDiagram>? Diagrams,
    List<LegacyDocument>? Documents, bool AsCopy);

public sealed record LegacySize(double Width, double Height);
public sealed record LegacyPoint(double X, double Y);
public sealed record LegacyLink(string Label, string Url);

public sealed record LegacyProperties(
    string? Name, string? Identifier, string? Description, string? Technology, string? Responsibility,
    string? Notes, string? PatternId, List<LegacyLink>? Links, string? LinkedDiagramId,
    Dictionary<string, string>? Metadata);

public sealed record LegacyReusableComponent(string Id, string TypeId, LegacySize? Size, LegacyProperties Properties);

public sealed record LegacyProject(
    string Id, string Name, string? Description, string? PrimaryStyleId, List<string>? SecondaryStyleIds,
    List<LegacyReusableComponent>? ReusableComponents);

public sealed record LegacyNode(
    string Id, string Type, LegacyPoint Position, LegacySize Size, int ZIndex, LegacyProperties Properties,
    string? ComponentId = null);

public sealed record LegacyEdge(
    string Id, string Source, string Target, string? SourceHandle, string? TargetHandle, string Kind,
    string? Label, string? Protocol, string? Description, string? Sync, string? Direction, List<LegacyPoint>? Waypoints);

public sealed record LegacyViewport(double X, double Y, double Zoom);
public sealed record LegacySettings(bool ShowGrid, bool SnapToGrid, int GridSize, bool ShowMinimap);

public sealed record LegacyDiagram(
    string Id, string Name, string? Description, string? Level, List<string>? StyleIds, List<string>? PatternIds,
    List<LegacyNode>? Nodes, List<LegacyEdge>? Edges, LegacyViewport? Viewport, LegacySettings? Settings);

public sealed record LegacyDocument(string Scope, string OwnerId, string Content);
