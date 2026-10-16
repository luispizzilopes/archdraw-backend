using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetDiagramsByProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetDiagramsByProject;

public record GetDiagramsByProjectQuery(Guid UserId, Guid ProjectId) : IRequest<ErrorOr<IReadOnlyList<DiagramResponse>>>;
