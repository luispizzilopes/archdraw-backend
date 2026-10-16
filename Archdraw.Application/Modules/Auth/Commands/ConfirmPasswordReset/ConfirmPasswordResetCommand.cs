using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.ConfirmPasswordReset;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public record ConfirmPasswordResetCommand(ConfirmPasswordResetRequest Request) : IRequest<ErrorOr<Success>>;
