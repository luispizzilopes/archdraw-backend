using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Projects.Mappers;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Projects.UseCases.GetProjectById;

public class GetProjectByIdUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<ProjectResponse>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        var count = await unitOfWork.DiagramRepository.CountByProjectAsync(projectId, cancellationToken);
        return project.ToResponse(count);
    }
}
