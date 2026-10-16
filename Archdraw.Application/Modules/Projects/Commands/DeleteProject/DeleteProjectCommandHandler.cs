using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.DeleteProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ErrorOr<Success>>
{
    private readonly DeleteProjectUseCase _deleteProjectUseCase;

    public DeleteProjectCommandHandler(DeleteProjectUseCase deleteProjectUseCase)
    {
        _deleteProjectUseCase = deleteProjectUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
        => await _deleteProjectUseCase.ExecuteAsync(command.UserId, command.ProjectId, cancellationToken);
}
