using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.CreateComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.CreateComponent;

public class CreateComponentCommandHandler : IRequestHandler<CreateComponentCommand, ErrorOr<ComponentResponse>>
{
    private readonly CreateComponentUseCase _createComponentUseCase;

    public CreateComponentCommandHandler(CreateComponentUseCase createComponentUseCase)
    {
        _createComponentUseCase = createComponentUseCase;
    }

    public async Task<ErrorOr<ComponentResponse>> Handle(CreateComponentCommand command, CancellationToken cancellationToken)
        => await _createComponentUseCase.ExecuteAsync(command.UserId, command.ProjectId, command.Request, cancellationToken);
}
