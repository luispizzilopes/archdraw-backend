using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramParents;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramParents;

public class GetDiagramParentsQueryHandler : IRequestHandler<GetDiagramParentsQuery, ErrorOr<IReadOnlyList<DiagramResponse>>>
{
    private readonly GetDiagramParentsUseCase _getDiagramParentsUseCase;

    public GetDiagramParentsQueryHandler(GetDiagramParentsUseCase getDiagramParentsUseCase)
    {
        _getDiagramParentsUseCase = getDiagramParentsUseCase;
    }

    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> Handle(GetDiagramParentsQuery query, CancellationToken cancellationToken)
        => await _getDiagramParentsUseCase.ExecuteAsync(query.UserId, query.DiagramId, cancellationToken);
}
