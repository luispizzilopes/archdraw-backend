using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.UpdateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.UpdateDiagram;

public class UpdateDiagramCommandHandler : IRequestHandler<UpdateDiagramCommand, ErrorOr<DiagramResponse>>
{
    private readonly UpdateDiagramUseCase _updateDiagramUseCase;

    public UpdateDiagramCommandHandler(UpdateDiagramUseCase updateDiagramUseCase)
    {
        _updateDiagramUseCase = updateDiagramUseCase;
    }

    public async Task<ErrorOr<DiagramResponse>> Handle(UpdateDiagramCommand command, CancellationToken cancellationToken)
        => await _updateDiagramUseCase.ExecuteAsync(command.UserId, command.DiagramId, command.Request, cancellationToken);
}
