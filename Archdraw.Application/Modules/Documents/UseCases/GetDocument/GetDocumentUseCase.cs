using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.Services;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Documents.UseCases.GetDocument;

public class GetDocumentUseCase(IUnitOfWork unitOfWork, DocumentOwnerResolver resolver) : IUseCase
{
    public async Task<ErrorOr<DocumentResponse>> ExecuteAsync(
        Guid userId, DocumentScope scope, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var projectId = await resolver.ResolveProjectIdAsync(userId, scope, ownerId, cancellationToken);
        if (projectId.IsError) return projectId.Errors;

        var document = await unitOfWork.DocumentRepository.GetAsync(scope, ownerId, cancellationToken);
        return document is null
            ? new DocumentResponse(scope.ToString(), ownerId, projectId.Value, string.Empty, 0, null)
            : new DocumentResponse(scope.ToString(), ownerId, document.ProjectId, document.Markdown, document.Revision, document.UpdatedAtUtc);
    }
}
