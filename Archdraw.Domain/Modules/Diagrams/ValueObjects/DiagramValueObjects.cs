namespace Archdraw.Domain.Modules.Diagrams.ValueObjects;

public sealed record Point(double X, double Y);

public sealed record Viewport(double X, double Y, double Zoom)
{
    public static Viewport Default { get; } = new(0, 0, 1);
}

public sealed record DiagramSettings(bool ShowGrid, bool SnapToGrid, int GridSize, bool ShowMinimap)
{
    public static DiagramSettings Default { get; } = new(true, true, 20, true);
}

public sealed record ComponentLink(string Label, string Url);

public sealed record NodeProperties(
    string Name,
    string Identifier,
    string Description,
    string Technology,
    string Responsibility,
    string Notes,
    string? PatternId,
    List<ComponentLink> Links,
    Dictionary<string, string> Metadata)
{
    public static NodeProperties Empty(string name) =>
        new(name, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, [], []);
}
