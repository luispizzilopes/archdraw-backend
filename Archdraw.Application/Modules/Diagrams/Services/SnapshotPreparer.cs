using ErrorOr;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Diagrams.Entities;

namespace Archdraw.Application.Modules.Diagrams.Services;

public class SnapshotPreparer(IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<(List<NodeData> Nodes, List<ConnectionData> Connections)>> PrepareAsync(
        Guid projectId, Guid diagramId, IReadOnlyCollection<NodeDto> nodes, IReadOnlyCollection<EdgeDto> edges,
        CancellationToken cancellationToken)
    {
        var linkedIds = nodes.Select(n => n.Properties.LinkedDiagramId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (linkedIds.Contains(diagramId))
            return Error.Validation("Diagram.SelfLink", "Um componente não pode apontar para o próprio diagrama.");
        if (linkedIds.Count > 0)
        {
            var existing = await unitOfWork.DiagramRepository.GetExistingIdsInProjectAsync(projectId, linkedIds, cancellationToken);
            if (existing.Count != linkedIds.Count)
                return Error.Validation("Diagram.LinkedDiagramInvalid", "Um componente aponta para um diagrama que não pertence a este projeto.");
        }

        var componentIds = nodes.Where(n => n.ComponentId.HasValue).Select(n => n.ComponentId!.Value).Distinct().ToList();
        var components = componentIds.Count == 0
            ? new Dictionary<Guid, Domain.Modules.Components.Aggregates.ArchitecturalComponent>()
            : (await unitOfWork.ComponentRepository.GetByIdsInProjectAsync(projectId, componentIds, cancellationToken))
                .ToDictionary(c => c.Id);
        if (components.Count != componentIds.Count)
            return Error.Validation("Component.Invalid", "Um nó referencia um componente que não pertence a este projeto.");

        var nodeData = nodes.Select(n =>
        {
            var data = n.ToData();
            return n.ComponentId is { } cid ? data with { Properties = components[cid].ToProperties() } : data;
        }).ToList();

        return (nodeData, edges.Select(e => e.ToData()).ToList());
    }
}
