using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramSnapshot;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramSnapshot;

public record GetDiagramSnapshotQuery(Guid UserId, Guid DiagramId) : IRequest<ErrorOr<DiagramSnapshotResponse>>;
