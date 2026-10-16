using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.ChangePassword;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, Guid? SessionId, ChangePasswordRequest Request) : IRequest<ErrorOr<Success>>;
