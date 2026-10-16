namespace Archdraw.Application.Modules.Projects.Responses;

public sealed record ProjectResponse(
    Guid Id, string Name, string Description, string? PrimaryStyleId, List<string> SecondaryStyleIds,
    int DiagramCount, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed record ImportProjectResponse(
    Guid ProjectId, bool AlreadyImported, int Diagrams, int Nodes, int Connections,
    int Components, int Documents, List<string> Warnings);

public sealed record DashboardSummaryResponse(int Projects, int Diagrams, int Components, int Documents);
