using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.UseCases.GetDocument;
using Archdraw.Domain.Modules.Documents.Enums;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Documents.Queries.GetDocument;

public record GetDocumentQuery(Guid UserId, DocumentScope Scope, Guid OwnerId) : IRequest<ErrorOr<DocumentResponse>>;
