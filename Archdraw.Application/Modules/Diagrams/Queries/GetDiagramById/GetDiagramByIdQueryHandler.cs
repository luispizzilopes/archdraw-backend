using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramById;

public class GetDiagramByIdQueryHandler : IRequestHandler<GetDiagramByIdQuery, ErrorOr<DiagramResponse>>
{
    private readonly GetDiagramByIdUseCase _getDiagramByIdUseCase;

    public GetDiagramByIdQueryHandler(GetDiagramByIdUseCase getDiagramByIdUseCase)
    {
        _getDiagramByIdUseCase = getDiagramByIdUseCase;
    }

    public async Task<ErrorOr<DiagramResponse>> Handle(GetDiagramByIdQuery query, CancellationToken cancellationToken)
        => await _getDiagramByIdUseCase.ExecuteAsync(query.UserId, query.DiagramId, cancellationToken);
}
