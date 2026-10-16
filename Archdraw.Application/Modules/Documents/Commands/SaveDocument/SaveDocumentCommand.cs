using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.UseCases.SaveDocument;
using Archdraw.Domain.Modules.Documents.Enums;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Documents.Commands.SaveDocument;

public record SaveDocumentCommand(Guid UserId, DocumentScope Scope, Guid OwnerId, SaveDocumentRequest Request) : IRequest<ErrorOr<DocumentResponse>>;
