using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Projects.UseCases.DeleteProject;

public class DeleteProjectUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        unitOfWork.ProjectRepository.Delete(project);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success;
    }
}
