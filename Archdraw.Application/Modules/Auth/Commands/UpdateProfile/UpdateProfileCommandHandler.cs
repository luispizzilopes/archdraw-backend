using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.UpdateProfile;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ErrorOr<GetProfileResponse>>
{
    private readonly UpdateProfileUseCase _updateProfileUseCase;

    public UpdateProfileCommandHandler(UpdateProfileUseCase updateProfileUseCase)
    {
        _updateProfileUseCase = updateProfileUseCase;
    }

    public async Task<ErrorOr<GetProfileResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        => await _updateProfileUseCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
