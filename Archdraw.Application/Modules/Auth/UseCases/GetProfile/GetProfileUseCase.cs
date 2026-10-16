using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.GetProfile;

public class GetProfileUseCase(UserManager<AppUser> userManager, IStorageService storageService) : IUseCase
{
    public async Task<ErrorOr<GetProfileResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return AppErrors.UserNotFound;

        return new GetProfileResponse(user.Id, user.Name, user.Email, storageService.GetPublicUrl(user.ProfilePhotoKey), user.CreatedAtUtc);
    }
}
