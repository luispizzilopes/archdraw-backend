using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Logout;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<ErrorOr<Success>>;
