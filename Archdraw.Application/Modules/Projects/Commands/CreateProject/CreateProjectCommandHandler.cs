using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.CreateProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ErrorOr<ProjectResponse>>
{
    private readonly CreateProjectUseCase _createProjectUseCase;

    public CreateProjectCommandHandler(CreateProjectUseCase createProjectUseCase)
    {
        _createProjectUseCase = createProjectUseCase;
    }

    public async Task<ErrorOr<ProjectResponse>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
        => await _createProjectUseCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
