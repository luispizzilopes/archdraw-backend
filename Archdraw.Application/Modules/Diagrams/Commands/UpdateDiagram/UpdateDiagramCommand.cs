using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.UpdateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.UpdateDiagram;

public record UpdateDiagramCommand(Guid UserId, Guid DiagramId, UpdateDiagramRequest Request) : IRequest<ErrorOr<DiagramResponse>>;
