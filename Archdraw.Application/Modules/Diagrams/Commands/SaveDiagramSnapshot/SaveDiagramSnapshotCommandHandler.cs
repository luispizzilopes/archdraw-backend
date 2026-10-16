using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.SaveDiagramSnapshot;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.SaveDiagramSnapshot;

public class SaveDiagramSnapshotCommandHandler : IRequestHandler<SaveDiagramSnapshotCommand, ErrorOr<SaveSnapshotResponse>>
{
    private readonly SaveDiagramSnapshotUseCase _saveDiagramSnapshotUseCase;

    public SaveDiagramSnapshotCommandHandler(SaveDiagramSnapshotUseCase saveDiagramSnapshotUseCase)
    {
        _saveDiagramSnapshotUseCase = saveDiagramSnapshotUseCase;
    }

    public async Task<ErrorOr<SaveSnapshotResponse>> Handle(SaveDiagramSnapshotCommand command, CancellationToken cancellationToken)
        => await _saveDiagramSnapshotUseCase.ExecuteAsync(command.UserId, command.DiagramId, command.Request, cancellationToken);
}
