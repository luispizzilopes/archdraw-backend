using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.GetComponentsByProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Queries.GetComponentsByProject;

public record GetComponentsByProjectQuery(Guid UserId, Guid ProjectId) : IRequest<ErrorOr<IReadOnlyList<ComponentResponse>>>;
