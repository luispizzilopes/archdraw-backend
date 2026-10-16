using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Components.Mappers;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Components.UseCases.GetComponentsByProject;

public class GetComponentsByProjectUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<IReadOnlyList<ComponentResponse>>> ExecuteAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        var components = await unitOfWork.ComponentRepository.ListByProjectAsync(projectId, userId, cancellationToken);
        var usage = await unitOfWork.ComponentRepository.GetUsageCountsAsync(projectId, cancellationToken);
        return components.Select(c => c.ToResponse(usage.GetValueOrDefault(c.Id))).ToList();
    }
}
