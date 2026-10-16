using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.SaveDiagramSnapshot;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.SaveDiagramSnapshot;

public record SaveDiagramSnapshotCommand(Guid UserId, Guid DiagramId, SaveSnapshotRequest Request) : IRequest<ErrorOr<SaveSnapshotResponse>>;
