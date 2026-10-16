using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramSnapshot;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramSnapshot;

public class GetDiagramSnapshotQueryHandler : IRequestHandler<GetDiagramSnapshotQuery, ErrorOr<DiagramSnapshotResponse>>
{
    private readonly GetDiagramSnapshotUseCase _getDiagramSnapshotUseCase;

    public GetDiagramSnapshotQueryHandler(GetDiagramSnapshotUseCase getDiagramSnapshotUseCase)
    {
        _getDiagramSnapshotUseCase = getDiagramSnapshotUseCase;
    }

    public async Task<ErrorOr<DiagramSnapshotResponse>> Handle(GetDiagramSnapshotQuery query, CancellationToken cancellationToken)
        => await _getDiagramSnapshotUseCase.ExecuteAsync(query.UserId, query.DiagramId, cancellationToken);
}
