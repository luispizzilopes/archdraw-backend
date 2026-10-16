using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.UpdateDiagram;

public class UpdateDiagramUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<DiagramResponse>> ExecuteAsync(
        Guid userId, Guid diagramId, UpdateDiagramRequest request, CancellationToken cancellationToken = default)
    {
        var diagram = await unitOfWork.DiagramRepository.GetByIdAsync(diagramId, userId, cancellationToken);
        if (diagram is null) return AppErrors.DiagramNotFound;

        diagram.UpdateMetadata(request.Name, request.Description, request.Level, request.StyleIds, request.PatternIds);
        await unitOfWork.CommitAsync(cancellationToken);

        var summary = await unitOfWork.DiagramRepository.GetSummaryAsync(diagramId, userId, cancellationToken);
        return summary!.ToResponse();
    }
}
