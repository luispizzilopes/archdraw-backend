using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Diagrams.UseCases.DuplicateDiagram;

public class DuplicateDiagramUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<DiagramResponse>> ExecuteAsync(
        Guid userId, Guid diagramId, DuplicateDiagramRequest request, CancellationToken cancellationToken = default)
    {
        var source = await unitOfWork.DiagramRepository.GetWithGraphReadOnlyAsync(diagramId, userId, cancellationToken);
        if (source is null) return AppErrors.DiagramNotFound;

        var name = string.IsNullOrWhiteSpace(request.Name) ? $"{source.Name} (cópia)" : request.Name.Trim();
        if (name.Length > Diagram.NameMaxLength) name = name[..Diagram.NameMaxLength];

        var copy = source.Duplicate(name);
        await unitOfWork.DiagramRepository.CreateAsync(copy, cancellationToken);

        var readme = await unitOfWork.DocumentRepository.GetAsync(DocumentScope.Diagram, diagramId, cancellationToken);
        if (readme is not null)
            await unitOfWork.DocumentRepository.CreateAsync(Document.Create(copy.ProjectId, DocumentScope.Diagram, copy.Id, readme.Markdown), cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return copy.ToResponse();
    }
}
