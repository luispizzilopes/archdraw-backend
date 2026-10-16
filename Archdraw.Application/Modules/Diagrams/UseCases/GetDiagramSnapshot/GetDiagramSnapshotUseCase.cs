using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramSnapshot;

public class GetDiagramSnapshotUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<DiagramSnapshotResponse>> ExecuteAsync(Guid userId, Guid diagramId, CancellationToken cancellationToken = default)
    {
        var diagram = await unitOfWork.DiagramRepository.GetWithGraphReadOnlyAsync(diagramId, userId, cancellationToken);
        if (diagram is null) return AppErrors.DiagramNotFound;

        var componentIds = diagram.Nodes.Where(n => n.ComponentId.HasValue).Select(n => n.ComponentId!.Value).Distinct().ToList();
        var components = componentIds.Count == 0
            ? new Dictionary<Guid, Domain.Modules.Components.Aggregates.ArchitecturalComponent>()
            : (await unitOfWork.ComponentRepository.GetByIdsInProjectAsync(diagram.ProjectId, componentIds, cancellationToken))
                .ToDictionary(c => c.Id);

        return new DiagramSnapshotResponse(
            diagram.Id, diagram.Revision, diagram.SchemaVersion,
            diagram.Nodes.OrderBy(n => n.ZIndex).Select(n => n.ToDto(n.ComponentId is { } cid && components.TryGetValue(cid, out var c) ? c : null)).ToList(),
            diagram.Connections.Select(c => c.ToDto()).ToList(),
            diagram.Viewport.ToDto(), diagram.Settings.ToDto(), diagram.UpdatedAtUtc);
    }
}
