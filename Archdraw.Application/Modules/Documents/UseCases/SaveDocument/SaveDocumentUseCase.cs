using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.Services;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Documents.UseCases.SaveDocument;

public class SaveDocumentUseCase(IUnitOfWork unitOfWork, DocumentOwnerResolver resolver) : IUseCase
{
    public const string RevisionConflictCode = "Document.RevisionConflict";

    public async Task<ErrorOr<DocumentResponse>> ExecuteAsync(
        Guid userId, DocumentScope scope, Guid ownerId, SaveDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var projectId = await resolver.ResolveProjectIdAsync(userId, scope, ownerId, cancellationToken);
        if (projectId.IsError) return projectId.Errors;

        var document = await unitOfWork.DocumentRepository.GetAsync(scope, ownerId, cancellationToken);
        if (document is null)
        {
            document = Document.Create(projectId.Value, scope, ownerId, request.Markdown);
            await unitOfWork.DocumentRepository.CreateAsync(document, cancellationToken);
        }
        else
        {
            if (!document.IsAtRevision(request.BaseRevision))
                return Error.Conflict(RevisionConflictCode, "O documento foi alterado por outra sessão.",
                    new Dictionary<string, object> { ["currentRevision"] = document.Revision });
            document.Update(request.Markdown);
        }

        try
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict(RevisionConflictCode, "O documento foi alterado por outra sessão.");
        }

        return new DocumentResponse(scope.ToString(), ownerId, document.ProjectId, document.Markdown, document.Revision, document.UpdatedAtUtc);
    }
}
