using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetRecentDiagrams;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetRecentDiagrams;

public class GetRecentDiagramsQueryHandler : IRequestHandler<GetRecentDiagramsQuery, ErrorOr<IReadOnlyList<DiagramResponse>>>
{
    private readonly GetRecentDiagramsUseCase _getRecentDiagramsUseCase;

    public GetRecentDiagramsQueryHandler(GetRecentDiagramsUseCase getRecentDiagramsUseCase)
    {
        _getRecentDiagramsUseCase = getRecentDiagramsUseCase;
    }

    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> Handle(GetRecentDiagramsQuery query, CancellationToken cancellationToken)
        => await _getRecentDiagramsUseCase.ExecuteAsync(query.UserId, query.Limit, cancellationToken);
}
