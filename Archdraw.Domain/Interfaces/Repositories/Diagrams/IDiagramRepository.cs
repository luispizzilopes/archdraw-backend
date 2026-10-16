using Archdraw.Domain.Modules.Diagrams.Aggregates;

namespace Archdraw.Domain.Interfaces.Repositories.Diagrams;

public interface IDiagramRepository
{
    Task CreateAsync(Diagram diagram, CancellationToken ct = default);

    Task<Diagram?> GetByIdAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default);

    Task<Diagram?> GetWithGraphAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default);

    Task<Diagram?> GetWithGraphReadOnlyAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<DiagramSummary>> ListByProjectAsync(Guid projectId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<DiagramSummary>> ListRecentAsync(Guid ownerId, int limit, CancellationToken ct = default);

    Task<IReadOnlyList<DiagramSummary>> ListParentsAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default);

    Task<DiagramSummary?> GetSummaryAsync(Guid diagramId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Guid>> GetExistingIdsInProjectAsync(Guid projectId, IEnumerable<Guid> ids, CancellationToken ct = default);

    Task<int> CountByProjectAsync(Guid projectId, CancellationToken ct = default);

    void Delete(Diagram diagram);
}

public sealed record DiagramSummary(
    Guid Id, Guid ProjectId, string Name, string Description, string Level,
    List<string> StyleIds, List<string> PatternIds, long Revision, int NodeCount, int ConnectionCount,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
