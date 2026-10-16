using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramsByProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramsByProject;

public class GetDiagramsByProjectQueryHandler : IRequestHandler<GetDiagramsByProjectQuery, ErrorOr<IReadOnlyList<DiagramResponse>>>
{
    private readonly GetDiagramsByProjectUseCase _getDiagramsByProjectUseCase;

    public GetDiagramsByProjectQueryHandler(GetDiagramsByProjectUseCase getDiagramsByProjectUseCase)
    {
        _getDiagramsByProjectUseCase = getDiagramsByProjectUseCase;
    }

    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> Handle(GetDiagramsByProjectQuery query, CancellationToken cancellationToken)
        => await _getDiagramsByProjectUseCase.ExecuteAsync(query.UserId, query.ProjectId, cancellationToken);
}
