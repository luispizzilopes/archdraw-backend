using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.UpdateComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.UpdateComponent;

public record UpdateComponentCommand(Guid UserId, Guid ComponentId, UpdateComponentRequest Request) : IRequest<ErrorOr<ComponentResponse>>;
