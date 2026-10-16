using Archdraw.Domain.Interfaces.Repositories.Components;
using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Repositories.Components;

public class ArchitecturalComponentRepository(AppDbContext context) : IArchitecturalComponentRepository
{
    public async Task CreateAsync(ArchitecturalComponent component, CancellationToken ct = default)
        => await context.ArchitecturalComponents.AddAsync(component, ct);

    public Task<ArchitecturalComponent?> GetByIdAsync(Guid componentId, Guid ownerId, CancellationToken ct = default)
        => context.ArchitecturalComponents
            .Where(c => context.Projects.Any(p => p.Id == c.ProjectId && p.OwnerId == ownerId))
            .FirstOrDefaultAsync(c => c.Id == componentId, ct);

    public async Task<IReadOnlyList<ArchitecturalComponent>> ListByProjectAsync(Guid projectId, Guid ownerId, CancellationToken ct = default)
        => await context.ArchitecturalComponents.AsNoTracking()
            .Where(c => c.ProjectId == projectId && context.Projects.Any(p => p.Id == projectId && p.OwnerId == ownerId))
            .OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<ArchitecturalComponent>> GetByIdsInProjectAsync(
        Guid projectId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();
        return await context.ArchitecturalComponents
            .Where(c => c.ProjectId == projectId && list.Contains(c.Id)).ToListAsync(ct);
    }

    public Task<int> CountUsagesAsync(Guid componentId, CancellationToken ct = default)
        => context.DiagramNodes.CountAsync(n => n.ComponentId == componentId, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetUsageCountsAsync(Guid projectId, CancellationToken ct = default)
        => await context.DiagramNodes.AsNoTracking()
            .Where(n => n.ComponentId != null && context.ArchitecturalComponents.Any(c => c.Id == n.ComponentId && c.ProjectId == projectId))
            .GroupBy(n => n.ComponentId!.Value)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public async Task DetachFromNodesAsync(Guid componentId, CancellationToken ct = default)
    {
        var nodes = await context.DiagramNodes.Where(n => n.ComponentId == componentId).ToListAsync(ct);
        foreach (var node in nodes) node.Detach();
    }

    public void Delete(ArchitecturalComponent component) => context.ArchitecturalComponents.Remove(component);
}
