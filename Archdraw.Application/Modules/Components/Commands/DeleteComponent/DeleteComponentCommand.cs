using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.DeleteComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.DeleteComponent;

public record DeleteComponentCommand(Guid UserId, Guid ComponentId, bool Detach) : IRequest<ErrorOr<Success>>;
