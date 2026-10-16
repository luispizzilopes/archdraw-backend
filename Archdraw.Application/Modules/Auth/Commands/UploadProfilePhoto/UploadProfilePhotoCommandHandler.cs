using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.UploadProfilePhoto;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.UploadProfilePhoto;

public class UploadProfilePhotoCommandHandler : IRequestHandler<UploadProfilePhotoCommand, ErrorOr<string>>
{
    private readonly UploadProfilePhotoUseCase _uploadProfilePhotoUseCase;

    public UploadProfilePhotoCommandHandler(UploadProfilePhotoUseCase uploadProfilePhotoUseCase)
    {
        _uploadProfilePhotoUseCase = uploadProfilePhotoUseCase;
    }

    public async Task<ErrorOr<string>> Handle(UploadProfilePhotoCommand command, CancellationToken cancellationToken)
        => await _uploadProfilePhotoUseCase.ExecuteAsync(command.UserId, command.File, cancellationToken);
}
