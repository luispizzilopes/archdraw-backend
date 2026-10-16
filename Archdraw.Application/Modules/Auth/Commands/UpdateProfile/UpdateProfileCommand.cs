using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.UpdateProfile;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(Guid UserId, UpdateProfileRequest Request) : IRequest<ErrorOr<GetProfileResponse>>;
