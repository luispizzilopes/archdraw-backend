using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.ImportProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.ImportProject;

public class ImportProjectCommandHandler : IRequestHandler<ImportProjectCommand, ErrorOr<ImportProjectResponse>>
{
    private readonly ImportProjectUseCase _importProjectUseCase;

    public ImportProjectCommandHandler(ImportProjectUseCase importProjectUseCase)
    {
        _importProjectUseCase = importProjectUseCase;
    }

    public async Task<ErrorOr<ImportProjectResponse>> Handle(ImportProjectCommand command, CancellationToken cancellationToken)
        => await _importProjectUseCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
