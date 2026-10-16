using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.UploadProfilePhoto;

public class UploadProfilePhotoUseCase(UserManager<AppUser> userManager, IStorageService storageService) : IUseCase
{
    public const long MaxBytes = 2 * 1024 * 1024;
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = "jpg", ["image/png"] = "png", ["image/webp"] = "webp"
    };

    public async Task<ErrorOr<string>> ExecuteAsync(Guid userId, FileUpload file, CancellationToken cancellationToken = default)
    {
        if (!Extensions.TryGetValue(file.ContentType, out var extension))
            return Error.Validation("Profile.PhotoType", "Envie uma imagem JPEG, PNG ou WebP.");
        if (file.Length is <= 0 or > MaxBytes)
            return Error.Validation("Profile.PhotoSize", "A foto deve ter até 2 MB.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return AppErrors.UserNotFound;

        var newKey = $"profile-photos/{userId}/{Guid.NewGuid()}.{extension}";
        await storageService.UploadPublicAsync(newKey, file.Content, file.ContentType, cancellationToken);

        var oldKey = user.ProfilePhotoKey;
        user.ProfilePhotoKey = newKey;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            await storageService.DeletePublicAsync(newKey, cancellationToken);
            return Services.IdentityErrorMapper.ToErrors(result);
        }

        if (!string.IsNullOrEmpty(oldKey)) await storageService.DeletePublicAsync(oldKey, cancellationToken);
        return storageService.GetPublicUrl(newKey)!;
    }
}
