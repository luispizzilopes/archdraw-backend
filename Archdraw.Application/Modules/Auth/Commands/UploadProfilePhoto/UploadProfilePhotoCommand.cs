using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.UploadProfilePhoto;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.UploadProfilePhoto;

public record UploadProfilePhotoCommand(Guid UserId, FileUpload File) : IRequest<ErrorOr<string>>;
