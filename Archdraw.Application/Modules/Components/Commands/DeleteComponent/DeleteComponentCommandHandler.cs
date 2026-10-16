using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.DeleteComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.DeleteComponent;

public class DeleteComponentCommandHandler : IRequestHandler<DeleteComponentCommand, ErrorOr<Success>>
{
    private readonly DeleteComponentUseCase _deleteComponentUseCase;

    public DeleteComponentCommandHandler(DeleteComponentUseCase deleteComponentUseCase)
    {
        _deleteComponentUseCase = deleteComponentUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteComponentCommand command, CancellationToken cancellationToken)
        => await _deleteComponentUseCase.ExecuteAsync(command.UserId, command.ComponentId, command.Detach, cancellationToken);
}
