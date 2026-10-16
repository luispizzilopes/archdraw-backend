using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramParents;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramParents;

public record GetDiagramParentsQuery(Guid UserId, Guid DiagramId) : IRequest<ErrorOr<IReadOnlyList<DiagramResponse>>>;
