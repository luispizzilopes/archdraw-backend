using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.DeleteDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.DeleteDiagram;

public class DeleteDiagramCommandHandler : IRequestHandler<DeleteDiagramCommand, ErrorOr<Success>>
{
    private readonly DeleteDiagramUseCase _deleteDiagramUseCase;

    public DeleteDiagramCommandHandler(DeleteDiagramUseCase deleteDiagramUseCase)
    {
        _deleteDiagramUseCase = deleteDiagramUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteDiagramCommand command, CancellationToken cancellationToken)
        => await _deleteDiagramUseCase.ExecuteAsync(command.UserId, command.DiagramId, cancellationToken);
}
