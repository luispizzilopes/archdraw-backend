using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.DeleteProfilePhoto;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.DeleteProfilePhoto;

public class DeleteProfilePhotoCommandHandler : IRequestHandler<DeleteProfilePhotoCommand, ErrorOr<Success>>
{
    private readonly DeleteProfilePhotoUseCase _deleteProfilePhotoUseCase;

    public DeleteProfilePhotoCommandHandler(DeleteProfilePhotoUseCase deleteProfilePhotoUseCase)
    {
        _deleteProfilePhotoUseCase = deleteProfilePhotoUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteProfilePhotoCommand command, CancellationToken cancellationToken)
        => await _deleteProfilePhotoUseCase.ExecuteAsync(command.UserId, cancellationToken);
}
