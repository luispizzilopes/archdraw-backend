using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Documents.Services;

public class DocumentOwnerResolver(IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<Guid>> ResolveProjectIdAsync(Guid userId, DocumentScope scope, Guid ownerId, CancellationToken ct)
    {
        switch (scope)
        {
            case DocumentScope.Project:
                var project = await unitOfWork.ProjectRepository.GetByIdAsync(ownerId, userId, ct);
                return project is null ? AppErrors.ProjectNotFound : project.Id;
            case DocumentScope.Diagram:
                var diagram = await unitOfWork.DiagramRepository.GetSummaryAsync(ownerId, userId, ct);
                return diagram is null ? AppErrors.DiagramNotFound : diagram.ProjectId;
            case DocumentScope.Component:
                var component = await unitOfWork.ComponentRepository.GetByIdAsync(ownerId, userId, ct);
                return component is null ? AppErrors.ComponentNotFound : component.ProjectId;
            default:
                return Error.Validation("Document.Scope", "Escopo de documento inválido.");
        }
    }
}
