using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.DuplicateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.DuplicateDiagram;

public class DuplicateDiagramCommandHandler : IRequestHandler<DuplicateDiagramCommand, ErrorOr<DiagramResponse>>
{
    private readonly DuplicateDiagramUseCase _duplicateDiagramUseCase;

    public DuplicateDiagramCommandHandler(DuplicateDiagramUseCase duplicateDiagramUseCase)
    {
        _duplicateDiagramUseCase = duplicateDiagramUseCase;
    }

    public async Task<ErrorOr<DiagramResponse>> Handle(DuplicateDiagramCommand command, CancellationToken cancellationToken)
        => await _duplicateDiagramUseCase.ExecuteAsync(command.UserId, command.DiagramId, command.Request, cancellationToken);
}
