using Archdraw.Domain.Interfaces.Repositories.Components;
using Archdraw.Domain.Interfaces.Repositories.Diagrams;
using Archdraw.Domain.Interfaces.Repositories.Documents;
using Archdraw.Domain.Interfaces.Repositories.Projects;
using Archdraw.Domain.Interfaces.Repositories.Users;

namespace Archdraw.Domain.Interfaces.UnitOfWork;

public interface IUnitOfWork
{
    IProjectRepository ProjectRepository { get; }
    IDiagramRepository DiagramRepository { get; }
    IArchitecturalComponentRepository ComponentRepository { get; }
    IDocumentRepository DocumentRepository { get; }
    IUserSessionRepository UserSessionRepository { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
