using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Register;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
