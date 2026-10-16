using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramsByProject;

public class GetDiagramsByProjectUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> ExecuteAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        var list = await unitOfWork.DiagramRepository.ListByProjectAsync(projectId, userId, cancellationToken);
        return list.Select(s => s.ToResponse()).ToList();
    }
}
