using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.DeleteProfilePhoto;

public class DeleteProfilePhotoUseCase(UserManager<AppUser> userManager, IStorageService storageService) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return AppErrors.UserNotFound;
        if (string.IsNullOrEmpty(user.ProfilePhotoKey)) return Result.Success;

        var key = user.ProfilePhotoKey;
        user.ProfilePhotoKey = null;
        await userManager.UpdateAsync(user);
        await storageService.DeletePublicAsync(key, cancellationToken);
        return Result.Success;
    }
}
