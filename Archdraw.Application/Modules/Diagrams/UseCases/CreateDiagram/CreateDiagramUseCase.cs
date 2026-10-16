using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.Services;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Diagrams.UseCases.CreateDiagram;

public class CreateDiagramUseCase(IUnitOfWork unitOfWork, SnapshotPreparer snapshotPreparer) : IUseCase
{
    public async Task<ErrorOr<DiagramResponse>> ExecuteAsync(
        Guid userId, Guid projectId, CreateDiagramRequest request, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        var id = request.Id ?? Guid.NewGuid();
        var existing = await unitOfWork.DiagramRepository.GetExistingIdsInProjectAsync(projectId, [id], cancellationToken);
        if (existing.Count > 0) return Error.Conflict("Diagram.AlreadyExists", "Já existe um diagrama com este identificador.");

        var diagram = Diagram.Create(id, projectId, request.Name, request.Description, request.Level, request.StyleIds, request.PatternIds);

        if (request.Snapshot is { } snapshot)
        {
            var prepared = await snapshotPreparer.PrepareAsync(projectId, id, snapshot.Nodes, snapshot.Edges, cancellationToken);
            if (prepared.IsError) return prepared.Errors;
            diagram.ApplySnapshot(prepared.Value.Nodes, prepared.Value.Connections, snapshot.Viewport.ToDomain(), snapshot.Settings.ToDomain());
        }

        await unitOfWork.DiagramRepository.CreateAsync(diagram, cancellationToken);
        if (request.Readme is not null)
            await unitOfWork.DocumentRepository.CreateAsync(Document.Create(projectId, DocumentScope.Diagram, id, request.Readme), cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return diagram.ToResponse();
    }
}
