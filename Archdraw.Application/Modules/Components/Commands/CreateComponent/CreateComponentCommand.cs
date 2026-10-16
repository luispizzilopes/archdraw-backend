using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.CreateComponent;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Commands.CreateComponent;

public record CreateComponentCommand(Guid UserId, Guid ProjectId, CreateComponentRequest Request) : IRequest<ErrorOr<ComponentResponse>>;
