using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Login;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
