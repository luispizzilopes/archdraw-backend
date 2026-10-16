using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Domain.Interfaces.Repositories.Documents;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken ct = default);
    Task<Document?> GetAsync(DocumentScope scope, Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> ListByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task DeleteByOwnerAsync(DocumentScope scope, Guid ownerId, CancellationToken ct = default);
}
