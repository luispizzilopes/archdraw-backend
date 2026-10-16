using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Interfaces.Repositories.Components;
using Archdraw.Domain.Interfaces.Repositories.Diagrams;
using Archdraw.Domain.Interfaces.Repositories.Documents;
using Archdraw.Domain.Interfaces.Repositories.Projects;
using Archdraw.Domain.Interfaces.Repositories.Users;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Infrastructure.Context;
using Archdraw.Infrastructure.Repositories.Components;
using Archdraw.Infrastructure.Repositories.Diagrams;
using Archdraw.Infrastructure.Repositories.Documents;
using Archdraw.Infrastructure.Repositories.Projects;
using Archdraw.Infrastructure.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Archdraw.Infrastructure.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IProjectRepository? _projects;
    private IDiagramRepository? _diagrams;
    private IArchitecturalComponentRepository? _components;
    private IDocumentRepository? _documents;
    private IUserSessionRepository? _sessions;

    public IProjectRepository ProjectRepository => _projects ??= new ProjectRepository(context);
    public IDiagramRepository DiagramRepository => _diagrams ??= new DiagramRepository(context);
    public IArchitecturalComponentRepository ComponentRepository => _components ??= new ArchitecturalComponentRepository(context);
    public IDocumentRepository DocumentRepository => _documents ??= new DocumentRepository(context);
    public IUserSessionRepository UserSessionRepository => _sessions ??= new UserSessionRepository(context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateResourceException("O recurso já existe.", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entities = string.Join(", ", ex.Entries.Select(e => $"{e.Metadata.ClrType.Name}:{e.State}"));
            throw new ConcurrencyConflictException($"O recurso foi alterado por outra operação. Entidades: {entities}", ex);
        }
    }
}
