using Archdraw.BuildingBlocks.Pagination;
using Archdraw.Domain.Modules.Projects.Aggregates;

namespace Archdraw.Domain.Interfaces.Repositories.Projects;

public interface IProjectRepository
{
    Task CreateAsync(Project project, CancellationToken ct = default);

    Task<Project?> GetByIdAsync(Guid projectId, Guid ownerId, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid projectId, CancellationToken ct = default);

    Task<(IReadOnlyList<ProjectListItem> Items, int Total)> GetPagedAsync(
        Guid ownerId, PaginationParams pagination, string? search, CancellationToken ct = default);

    Task<OwnerSummary> GetOwnerSummaryAsync(Guid ownerId, CancellationToken ct = default);

    void Delete(Project project);
}

public sealed record ProjectListItem(Project Project, int DiagramCount);

public sealed record OwnerSummary(int Projects, int Diagrams, int Components, int Documents);
