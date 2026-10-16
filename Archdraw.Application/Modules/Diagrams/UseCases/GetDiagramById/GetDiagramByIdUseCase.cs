using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramById;

public class GetDiagramByIdUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<DiagramResponse>> ExecuteAsync(Guid userId, Guid diagramId, CancellationToken cancellationToken = default)
    {
        var summary = await unitOfWork.DiagramRepository.GetSummaryAsync(diagramId, userId, cancellationToken);
        return summary is null ? AppErrors.DiagramNotFound : summary.ToResponse();
    }
}
