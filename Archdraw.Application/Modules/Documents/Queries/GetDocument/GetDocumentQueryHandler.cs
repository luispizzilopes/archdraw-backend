using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.UseCases.GetDocument;
using Archdraw.Domain.Modules.Documents.Enums;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Documents.Queries.GetDocument;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, ErrorOr<DocumentResponse>>
{
    private readonly GetDocumentUseCase _getDocumentUseCase;

    public GetDocumentQueryHandler(GetDocumentUseCase getDocumentUseCase)
    {
        _getDocumentUseCase = getDocumentUseCase;
    }

    public async Task<ErrorOr<DocumentResponse>> Handle(GetDocumentQuery query, CancellationToken cancellationToken)
        => await _getDocumentUseCase.ExecuteAsync(query.UserId, query.Scope, query.OwnerId, cancellationToken);
}
