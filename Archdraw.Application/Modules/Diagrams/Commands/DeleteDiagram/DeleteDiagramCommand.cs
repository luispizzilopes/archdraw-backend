using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.DeleteDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.DeleteDiagram;

public record DeleteDiagramCommand(Guid UserId, Guid DiagramId) : IRequest<ErrorOr<Success>>;
