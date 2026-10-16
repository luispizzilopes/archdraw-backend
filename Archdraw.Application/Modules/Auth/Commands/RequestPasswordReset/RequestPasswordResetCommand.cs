using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.RequestPasswordReset;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.RequestPasswordReset;

public record RequestPasswordResetCommand(RequestPasswordResetRequest Request) : IRequest<ErrorOr<Success>>;
