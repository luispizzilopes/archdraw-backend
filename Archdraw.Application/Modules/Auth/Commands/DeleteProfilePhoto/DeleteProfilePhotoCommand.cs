using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.DeleteProfilePhoto;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.DeleteProfilePhoto;

public record DeleteProfilePhotoCommand(Guid UserId) : IRequest<ErrorOr<Success>>;
