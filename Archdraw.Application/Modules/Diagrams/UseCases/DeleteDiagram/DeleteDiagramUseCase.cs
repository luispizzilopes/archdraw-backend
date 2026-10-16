using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Diagrams.UseCases.DeleteDiagram;

public class DeleteDiagramUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(Guid userId, Guid diagramId, CancellationToken cancellationToken = default)
    {
        var diagram = await unitOfWork.DiagramRepository.GetByIdAsync(diagramId, userId, cancellationToken);
        if (diagram is null) return AppErrors.DiagramNotFound;

        await unitOfWork.DocumentRepository.DeleteByOwnerAsync(DocumentScope.Diagram, diagramId, cancellationToken);
        unitOfWork.DiagramRepository.Delete(diagram);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success;
    }
}
