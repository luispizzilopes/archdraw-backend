using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.RefreshSession;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(string RefreshToken, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
