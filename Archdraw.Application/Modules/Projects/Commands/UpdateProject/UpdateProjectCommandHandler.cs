using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.UpdateProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ErrorOr<ProjectResponse>>
{
    private readonly UpdateProjectUseCase _updateProjectUseCase;

    public UpdateProjectCommandHandler(UpdateProjectUseCase updateProjectUseCase)
    {
        _updateProjectUseCase = updateProjectUseCase;
    }

    public async Task<ErrorOr<ProjectResponse>> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
        => await _updateProjectUseCase.ExecuteAsync(command.UserId, command.ProjectId, command.Request, cancellationToken);
}
