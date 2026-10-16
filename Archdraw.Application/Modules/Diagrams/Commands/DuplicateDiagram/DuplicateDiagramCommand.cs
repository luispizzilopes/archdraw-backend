using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.DuplicateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.DuplicateDiagram;

public record DuplicateDiagramCommand(Guid UserId, Guid DiagramId, DuplicateDiagramRequest Request) : IRequest<ErrorOr<DiagramResponse>>;
