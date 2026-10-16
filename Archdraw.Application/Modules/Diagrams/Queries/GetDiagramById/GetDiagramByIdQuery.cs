using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramById;

public record GetDiagramByIdQuery(Guid UserId, Guid DiagramId) : IRequest<ErrorOr<DiagramResponse>>;
