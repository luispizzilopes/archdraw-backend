using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Services;

namespace Archdraw.Application.Modules.Auth.UseCases.Logout;

public class LogoutUseCase(AuthSessionService authSessionService) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await authSessionService.RevokeByRefreshTokenAsync(refreshToken, cancellationToken);
        return Result.Success;
    }
}
