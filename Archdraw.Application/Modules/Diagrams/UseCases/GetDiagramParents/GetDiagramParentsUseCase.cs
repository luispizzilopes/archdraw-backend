using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramParents;

public class GetDiagramParentsUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> ExecuteAsync(Guid userId, Guid diagramId, CancellationToken cancellationToken = default)
    {
        var summary = await unitOfWork.DiagramRepository.GetSummaryAsync(diagramId, userId, cancellationToken);
        if (summary is null) return AppErrors.DiagramNotFound;
        return (await unitOfWork.DiagramRepository.ListParentsAsync(diagramId, userId, cancellationToken)).Select(s => s.ToResponse()).ToList();
    }
}
