using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Domain.Modules.Projects.Aggregates;

namespace Archdraw.Application.Modules.Projects.Mappers;

public static class ProjectMapper
{
    public static ProjectResponse ToResponse(this Project p, int diagramCount) => new(
        p.Id, p.Name, p.Description, p.PrimaryStyleId, p.SecondaryStyleIds, diagramCount, p.CreatedAtUtc, p.UpdatedAtUtc);
}
