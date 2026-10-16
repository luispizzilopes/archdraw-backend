using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Projects.Mappers;
using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Projects.Aggregates;

namespace Archdraw.Application.Modules.Projects.UseCases.CreateProject;

public class CreateProjectUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<ProjectResponse>> ExecuteAsync(
        Guid userId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var id = request.Id ?? Guid.NewGuid();
        if (await unitOfWork.ProjectRepository.ExistsAsync(id, cancellationToken))
            return Error.Conflict("Project.AlreadyExists", "Já existe um projeto com este identificador.");

        var project = Project.Create(id, userId, request.Name, request.Description, request.PrimaryStyleId, request.SecondaryStyleIds);
        await unitOfWork.ProjectRepository.CreateAsync(project, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return project.ToResponse(0);
    }
}
