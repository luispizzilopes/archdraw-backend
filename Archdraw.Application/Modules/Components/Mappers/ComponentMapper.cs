using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Domain.Modules.Components.Aggregates;

namespace Archdraw.Application.Modules.Components.Mappers;

public static class ComponentMapper
{
    public static ComponentResponse ToResponse(this ArchitecturalComponent c, int usageCount) => new(
        c.Id, c.ProjectId, c.TypeId, c.ToProperties().ToDto(null), usageCount, c.CreatedAtUtc, c.UpdatedAtUtc);
}
