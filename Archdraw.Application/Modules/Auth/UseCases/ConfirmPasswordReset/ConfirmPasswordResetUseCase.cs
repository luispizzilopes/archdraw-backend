using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.ConfirmPasswordReset;

public class ConfirmPasswordResetUseCase(
    UserManager<AppUser> userManager, IPasswordResetCodeStore codeStore, IUnitOfWork unitOfWork) : IUseCase
{
    private const int MaxAttempts = 5;
    private static Error InvalidCode => Error.Validation("Auth.InvalidCode", "Código inválido ou expirado.");

    public async Task<ErrorOr<Success>> ExecuteAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var stored = await codeStore.GetAsync(email, cancellationToken);
        if (stored is null) return InvalidCode;

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(request.Code)))
        {
            if (await codeStore.RegisterFailedAttemptAsync(email, cancellationToken) >= MaxAttempts)
                await codeStore.RemoveAsync(email, cancellationToken);
            return InvalidCode;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return InvalidCode;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded) return IdentityErrorMapper.ToErrors(result);

        await codeStore.RemoveAsync(email, cancellationToken);
        await unitOfWork.UserSessionRepository.RevokeAllAsync(user.Id, "Sessões encerradas após redefinição de senha.", null, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success;
    }
}
