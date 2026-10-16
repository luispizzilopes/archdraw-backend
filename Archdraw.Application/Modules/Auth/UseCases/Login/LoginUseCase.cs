using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.Login;

public class LoginUseCase(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AuthSessionService authSessionService) : IUseCase
{
    private static Error InvalidCredentials => Error.Unauthorized("Auth.InvalidCredentials", "E-mail ou senha inválidos.");
    private static Error LockedOut => Error.Unauthorized("Auth.LockedOut", "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.");

    public async Task<ErrorOr<LoginResponse>> ExecuteAsync(
        LoginRequest request, SessionMetadata metadata, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return InvalidCredentials;
        if (!user.Active) return Error.Unauthorized("Auth.Inactive", "Usuário está inativo.");
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow) return LockedOut;

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            user.LockoutLevel++;
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(GetLockoutDuration(user.LockoutLevel)));
            await userManager.UpdateAsync(user);
            return LockedOut;
        }
        if (!result.Succeeded) return InvalidCredentials;

        user.LastLoginAt = DateTime.UtcNow;
        user.LockoutLevel = 0;
        await userManager.UpdateAsync(user);

        return await authSessionService.CreateAsync(user, metadata, cancellationToken);
    }

    private static TimeSpan GetLockoutDuration(int level) => level switch
    {
        <= 1 => TimeSpan.FromMinutes(5),
        2 => TimeSpan.FromMinutes(15),
        3 => TimeSpan.FromHours(1),
        _ => TimeSpan.FromHours(24)
    };
}
