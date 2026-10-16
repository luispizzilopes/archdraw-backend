using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.GetComponentById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Queries.GetComponentById;

public record GetComponentByIdQuery(Guid UserId, Guid ComponentId) : IRequest<ErrorOr<ComponentResponse>>;
