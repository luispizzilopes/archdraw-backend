using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.ChangePassword;

public class ChangePasswordUseCase(UserManager<AppUser> userManager, IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(Guid userId, Guid? sessionId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return AppErrors.UserNotFound;

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) return IdentityErrorMapper.ToErrors(result);

        await unitOfWork.UserSessionRepository.RevokeAllAsync(userId, "Senha alterada.", sessionId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success;
    }
}
