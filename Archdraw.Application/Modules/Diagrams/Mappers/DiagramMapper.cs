using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.Repositories.Diagrams;
using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Application.Modules.Diagrams.Mappers;

public static class DiagramMapper
{
    public static NodeProperties ToDomain(this NodePropertiesDto p) => new(
        p.Name ?? string.Empty,
        p.Identifier ?? string.Empty,
        p.Description ?? string.Empty,
        p.Technology ?? string.Empty,
        p.Responsibility ?? string.Empty,
        p.Notes ?? string.Empty,
        string.IsNullOrWhiteSpace(p.PatternId) ? null : p.PatternId,
        (p.Links ?? []).Select(l => new ComponentLink(l.Label ?? string.Empty, l.Url ?? string.Empty)).ToList(),
        new Dictionary<string, string>(p.Metadata ?? []));

    public static NodePropertiesDto ToDto(this NodeProperties p, Guid? linkedDiagramId) => new(
        p.Name, p.Identifier, p.Description, p.Technology, p.Responsibility, p.Notes, p.PatternId,
        p.Links.Select(l => new ComponentLinkDto(l.Label, l.Url)).ToList(), linkedDiagramId,
        new Dictionary<string, string>(p.Metadata));

    public static NodeData ToData(this NodeDto n) => new(
        n.Id, n.Type, n.Position.X, n.Position.Y, n.Size.Width, n.Size.Height, n.ZIndex,
        n.ComponentId, n.Properties.LinkedDiagramId, n.Properties.ToDomain());

    public static ConnectionData ToData(this EdgeDto e) => new(
        e.Id, e.Source, e.Target, e.SourceHandle, e.TargetHandle, e.Kind, e.Label ?? string.Empty,
        e.Protocol ?? string.Empty, e.Description ?? string.Empty, e.Sync, e.Direction,
        (e.Waypoints ?? []).Select(p => new Point(p.X, p.Y)).ToList());

    public static NodeDto ToDto(this DiagramNode n, ArchitecturalComponent? component = null) => new(
        n.Id, n.TypeId, new PointDto(n.X, n.Y), new SizeDto(n.Width, n.Height), n.ZIndex, n.ComponentId,
        (component is null ? n.Properties : component.ToProperties()).ToDto(n.LinkedDiagramId));

    public static EdgeDto ToDto(this DiagramConnection c) => new(
        c.Id, c.SourceNodeId, c.TargetNodeId, c.SourceHandle, c.TargetHandle, c.KindId, c.Label, c.Protocol,
        c.Description, c.Sync, c.Direction, c.Waypoints.Select(p => new PointDto(p.X, p.Y)).ToList());

    public static ViewportDto ToDto(this Viewport v) => new(v.X, v.Y, v.Zoom);
    public static DiagramSettingsDto ToDto(this DiagramSettings s) => new(s.ShowGrid, s.SnapToGrid, s.GridSize, s.ShowMinimap);
    public static Viewport ToDomain(this ViewportDto v) => new(v.X, v.Y, v.Zoom);
    public static DiagramSettings ToDomain(this DiagramSettingsDto s) => new(s.ShowGrid, s.SnapToGrid, s.GridSize, s.ShowMinimap);

    public static DiagramResponse ToResponse(this DiagramSummary s) => new(
        s.Id, s.ProjectId, s.Name, s.Description, s.Level, s.StyleIds, s.PatternIds, s.Revision,
        s.NodeCount, s.ConnectionCount, s.CreatedAtUtc, s.UpdatedAtUtc);

    public static DiagramResponse ToResponse(this Diagram d) => new(
        d.Id, d.ProjectId, d.Name, d.Description, d.Level, d.StyleIds, d.PatternIds, d.Revision,
        d.Nodes.Count, d.Connections.Count, d.CreatedAtUtc, d.UpdatedAtUtc);
}
