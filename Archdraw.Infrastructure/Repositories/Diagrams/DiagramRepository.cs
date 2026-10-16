using Archdraw.Domain.Interfaces.Repositories.Diagrams;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Repositories.Diagrams;

public class DiagramRepository(AppDbContext context) : IDiagramRepository
{
    private IQueryable<Diagram> Owned(Guid ownerId)
        => context.Diagrams.Where(d => context.Projects.Any(p => p.Id == d.ProjectId && p.OwnerId == ownerId));

    public async Task CreateAsync(Diagram diagram, CancellationToken ct = default)
        => await context.Diagrams.AddAsync(diagram, ct);

    public Task<Diagram?> GetByIdAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default)
        => Owned(ownerId).FirstOrDefaultAsync(d => d.Id == diagramId, ct);

    public Task<Diagram?> GetWithGraphAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default)
        => Owned(ownerId).Include(d => d.Nodes).Include(d => d.Connections)
            .AsSplitQuery().FirstOrDefaultAsync(d => d.Id == diagramId, ct);

    public Task<Diagram?> GetWithGraphReadOnlyAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default)
        => Owned(ownerId).AsNoTracking().Include(d => d.Nodes).Include(d => d.Connections)
            .AsSplitQuery().FirstOrDefaultAsync(d => d.Id == diagramId, ct);

    private static async Task<IReadOnlyList<DiagramSummary>> ToSummariesAsync(IQueryable<Diagram> query, CancellationToken ct)
    {
        var rows = await query.AsNoTracking()
            .Select(d => new
            {
                d.Id, d.ProjectId, d.Name, d.Description, d.Level, d.StyleIds, d.PatternIds, d.Revision,
                Nodes = d.Nodes.Count, Connections = d.Connections.Count, d.CreatedAtUtc, d.UpdatedAtUtc
            })
            .ToListAsync(ct);

        return rows.Select(r => new DiagramSummary(
            r.Id, r.ProjectId, r.Name, r.Description, r.Level, r.StyleIds, r.PatternIds, r.Revision,
            r.Nodes, r.Connections, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList();
    }

    public Task<IReadOnlyList<DiagramSummary>> ListByProjectAsync(Guid projectId, Guid ownerId, CancellationToken ct = default)
        => ToSummariesAsync(Owned(ownerId).Where(d => d.ProjectId == projectId).OrderByDescending(d => d.UpdatedAtUtc), ct);

    public Task<IReadOnlyList<DiagramSummary>> ListRecentAsync(Guid ownerId, int limit, CancellationToken ct = default)
        => ToSummariesAsync(Owned(ownerId).OrderByDescending(d => d.UpdatedAtUtc).Take(Math.Clamp(limit, 1, 50)), ct);

    public Task<IReadOnlyList<DiagramSummary>> ListParentsAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default)
        => ToSummariesAsync(
            Owned(ownerId).Where(d => d.Id != diagramId && d.Nodes.Any(n => n.LinkedDiagramId == diagramId)).OrderBy(d => d.Name), ct);

    public async Task<DiagramSummary?> GetSummaryAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default)
    {
        var r = await Owned(ownerId).AsNoTracking().Where(d => d.Id == diagramId)
            .Select(d => new
            {
                d.Id, d.ProjectId, d.Name, d.Description, d.Level, d.StyleIds, d.PatternIds, d.Revision,
                Nodes = d.Nodes.Count, Connections = d.Connections.Count, d.CreatedAtUtc, d.UpdatedAtUtc
            }).FirstOrDefaultAsync(ct);
        return r is null ? null : new DiagramSummary(
            r.Id, r.ProjectId, r.Name, r.Description, r.Level, r.StyleIds, r.PatternIds, r.Revision,
            r.Nodes, r.Connections, r.CreatedAtUtc, r.UpdatedAtUtc);
    }

    public async Task<IReadOnlyCollection<Guid>> GetExistingIdsInProjectAsync(
        Guid projectId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();
        return await context.Diagrams.AsNoTracking()
            .Where(d => d.ProjectId == projectId && list.Contains(d.Id))
            .Select(d => d.Id).ToListAsync(ct);
    }

    public Task<int> CountByProjectAsync(Guid projectId, CancellationToken ct = default)
        => context.Diagrams.CountAsync(d => d.ProjectId == projectId, ct);

    public void Delete(Diagram diagram) => context.Diagrams.Remove(diagram);
}
