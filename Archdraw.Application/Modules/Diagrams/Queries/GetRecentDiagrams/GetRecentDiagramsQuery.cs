using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Application.Modules.Diagrams.UseCases.GetRecentDiagrams;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Diagrams.Queries.GetRecentDiagrams;

public record GetRecentDiagramsQuery(Guid UserId, int Limit) : IRequest<ErrorOr<IReadOnlyList<DiagramResponse>>>;
