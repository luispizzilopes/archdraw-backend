using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.UpdateProfile;

public class UpdateProfileUseCase(UserManager<AppUser> userManager, IStorageService storageService) : IUseCase
{
    public async Task<ErrorOr<GetProfileResponse>> ExecuteAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return AppErrors.UserNotFound;

        user.Name = request.Name.Trim();
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return IdentityErrorMapper.ToErrors(result);

        return new GetProfileResponse(user.Id, user.Name, user.Email, storageService.GetPublicUrl(user.ProfilePhotoKey), user.CreatedAtUtc);
    }
}
