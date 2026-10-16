using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.UpdateComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.UpdateComponent;

public class UpdateComponentCommandHandler : IRequestHandler<UpdateComponentCommand, ErrorOr<ComponentResponse>>
{
    private readonly UpdateComponentUseCase _updateComponentUseCase;

    public UpdateComponentCommandHandler(UpdateComponentUseCase updateComponentUseCase)
    {
        _updateComponentUseCase = updateComponentUseCase;
    }

    public async Task<ErrorOr<ComponentResponse>> Handle(UpdateComponentCommand command, CancellationToken cancellationToken)
        => await _updateComponentUseCase.ExecuteAsync(command.UserId, command.ComponentId, command.Request, cancellationToken);
}
