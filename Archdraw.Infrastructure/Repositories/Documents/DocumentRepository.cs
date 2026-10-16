using Archdraw.Domain.Interfaces.Repositories.Documents;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;
using Archdraw.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Repositories.Documents;

public class DocumentRepository(AppDbContext context) : IDocumentRepository
{
    public async Task CreateAsync(Document document, CancellationToken ct = default)
        => await context.Documents.AddAsync(document, ct);

    public Task<Document?> GetAsync(DocumentScope scope, Guid ownerId, CancellationToken ct = default)
        => context.Documents.FirstOrDefaultAsync(d => d.Scope == scope && d.OwnerId == ownerId, ct);

    public async Task<IReadOnlyList<Document>> ListByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await context.Documents.AsNoTracking().Where(d => d.ProjectId == projectId).ToListAsync(ct);

    public async Task DeleteByOwnerAsync(DocumentScope scope, Guid ownerId, CancellationToken ct = default)
    {
        var docs = await context.Documents.Where(d => d.Scope == scope && d.OwnerId == ownerId).ToListAsync(ct);
        context.Documents.RemoveRange(docs);
    }
}
