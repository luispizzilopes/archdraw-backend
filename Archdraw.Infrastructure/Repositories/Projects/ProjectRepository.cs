using Archdraw.BuildingBlocks.Pagination;
using Archdraw.Domain.Interfaces.Repositories.Projects;
using Archdraw.Domain.Modules.Projects.Aggregates;
using Archdraw.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Repositories.Projects;

public class ProjectRepository(AppDbContext context) : IProjectRepository
{
    public async Task CreateAsync(Project project, CancellationToken ct = default)
        => await context.Projects.AddAsync(project, ct);

    public Task<Project?> GetByIdAsync(Guid projectId, Guid ownerId, CancellationToken ct = default)
        => context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == ownerId, ct);

    public Task<bool> ExistsAsync(Guid projectId, CancellationToken ct = default)
        => context.Projects.AnyAsync(p => p.Id == projectId, ct);

    public async Task<(IReadOnlyList<ProjectListItem> Items, int Total)> GetPagedAsync(
        Guid ownerId, PaginationParams pagination, string? search, CancellationToken ct = default)
    {
        var query = context.Projects.AsNoTracking().Where(p => p.OwnerId == ownerId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Description, pattern));
        }

        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(p => p.UpdatedAtUtc)
            .Skip(pagination.Skip).Take(pagination.FinalSize)
            .Select(p => new { Project = p, Count = context.Diagrams.Count(d => d.ProjectId == p.Id) })
            .ToListAsync(ct);
        return (rows.Select(r => new ProjectListItem(r.Project, r.Count)).ToList(), total);
    }

    public async Task<OwnerSummary> GetOwnerSummaryAsync(Guid ownerId, CancellationToken ct = default)
    {
        var projects = await context.Projects.CountAsync(p => p.OwnerId == ownerId, ct);
        var diagrams = await context.Diagrams.CountAsync(d => context.Projects.Any(p => p.Id == d.ProjectId && p.OwnerId == ownerId), ct);
        var components = await context.ArchitecturalComponents.CountAsync(c => context.Projects.Any(p => p.Id == c.ProjectId && p.OwnerId == ownerId), ct);
        var documents = await context.Documents.CountAsync(
            d => d.Markdown.Trim() != "" && context.Projects.Any(p => p.Id == d.ProjectId && p.OwnerId == ownerId), ct);
        return new OwnerSummary(projects, diagrams, components, documents);
    }

    public void Delete(Project project) => context.Projects.Remove(project);
}
