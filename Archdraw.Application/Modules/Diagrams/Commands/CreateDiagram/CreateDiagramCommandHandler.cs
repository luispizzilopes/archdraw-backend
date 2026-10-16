using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.CreateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.CreateDiagram;

public class CreateDiagramCommandHandler : IRequestHandler<CreateDiagramCommand, ErrorOr<DiagramResponse>>
{
    private readonly CreateDiagramUseCase _createDiagramUseCase;

    public CreateDiagramCommandHandler(CreateDiagramUseCase createDiagramUseCase)
    {
        _createDiagramUseCase = createDiagramUseCase;
    }

    public async Task<ErrorOr<DiagramResponse>> Handle(CreateDiagramCommand command, CancellationToken cancellationToken)
        => await _createDiagramUseCase.ExecuteAsync(command.UserId, command.ProjectId, command.Request, cancellationToken);
}
