using Archdraw.Domain.Modules.Components.Aggregates;

namespace Archdraw.Domain.Interfaces.Repositories.Components;

public interface IArchitecturalComponentRepository
{
    Task CreateAsync(ArchitecturalComponent component, CancellationToken ct = default);
    Task<ArchitecturalComponent?> GetByIdAsync(Guid componentId, Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<ArchitecturalComponent>> ListByProjectAsync(Guid projectId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<ArchitecturalComponent>> GetByIdsInProjectAsync(Guid projectId, IEnumerable<Guid> ids, CancellationToken ct = default);

    Task<int> CountUsagesAsync(Guid componentId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> GetUsageCountsAsync(Guid projectId, CancellationToken ct = default);

    Task DetachFromNodesAsync(Guid componentId, CancellationToken ct = default);

    void Delete(ArchitecturalComponent component);
}
