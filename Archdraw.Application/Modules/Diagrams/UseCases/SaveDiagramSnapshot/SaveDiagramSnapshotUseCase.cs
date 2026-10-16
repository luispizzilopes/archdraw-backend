using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.Services;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.SaveDiagramSnapshot;

public class SaveDiagramSnapshotUseCase(IUnitOfWork unitOfWork, SnapshotPreparer snapshotPreparer) : IUseCase
{
    public const string RevisionConflictCode = "Diagram.RevisionConflict";

    public async Task<ErrorOr<SaveSnapshotResponse>> ExecuteAsync(
        Guid userId, Guid diagramId, SaveSnapshotRequest request, CancellationToken cancellationToken = default)
    {
        var diagram = await unitOfWork.DiagramRepository.GetWithGraphAsync(diagramId, userId, cancellationToken);
        if (diagram is null) return AppErrors.DiagramNotFound;

        if (!diagram.IsAtRevision(request.BaseRevision))
            return AppErrors.RevisionConflict(RevisionConflictCode, diagram.Revision);

        var prepared = await snapshotPreparer.PrepareAsync(diagram.ProjectId, diagramId, request.Nodes, request.Edges, cancellationToken);
        if (prepared.IsError) return prepared.Errors;

        diagram.ApplySnapshot(prepared.Value.Nodes, prepared.Value.Connections, request.Viewport.ToDomain(), request.Settings.ToDomain());

        try
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict(RevisionConflictCode, "O diagrama foi alterado por outra sessão. Recarregue ou escolha qual versão manter.");
        }

        return new SaveSnapshotResponse(diagram.Revision, diagram.UpdatedAtUtc);
    }
}
