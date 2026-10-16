using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.CreateDiagram;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Commands.CreateDiagram;

public record CreateDiagramCommand(Guid UserId, Guid ProjectId, CreateDiagramRequest Request) : IRequest<ErrorOr<DiagramResponse>>;
