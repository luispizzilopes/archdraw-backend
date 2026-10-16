using ErrorOr;
using Archdraw.Application.Interfaces.Services;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Archdraw.Application.Modules.Auth.Services;

public class AuthSessionService(
    IUnitOfWork unitOfWork,
    ITokenJwtService tokenService,
    IStorageService storageService,
    UserManager<AppUser> userManager,
    IConfiguration configuration)
{
    public async Task<LoginResponse> CreateAsync(AppUser user, SessionMetadata metadata, CancellationToken cancellationToken)
    {
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays());
        var session = UserSession.Create(
            user.Id, tokenService.HashRefreshToken(refreshToken), metadata.DeviceName,
            metadata.UserAgent, metadata.IpAddress, refreshExpiresAtUtc);

        await unitOfWork.UserSessionRepository.CreateAsync(session, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return BuildResponse(user, session.FamilyId, refreshToken, refreshExpiresAtUtc);
    }

    public async Task<ErrorOr<LoginResponse>> RefreshAsync(string refreshToken, SessionMetadata metadata, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(refreshToken);
        var current = await unitOfWork.UserSessionRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (current is null) return InvalidSession();

        if (current.RevokedAtUtc is not null)
        {
            if (current.ReplacedBySessionId.HasValue)
            {
                await unitOfWork.UserSessionRepository.RevokeAllAsync(
                    current.UserId, "Reutilização de token de atualização detectada.", null, cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            return InvalidSession();
        }

        if (current.ExpiresAtUtc <= DateTime.UtcNow)
        {
            current.Revoke("Token de atualização expirado.");
            await unitOfWork.CommitAsync(cancellationToken);
            return InvalidSession();
        }

        var user = await userManager.FindByIdAsync(current.UserId.ToString());
        if (user is null || !user.Active)
        {
            current.Revoke("Usuário inexistente ou inativo.");
            await unitOfWork.CommitAsync(cancellationToken);
            return InvalidSession();
        }

        var newRefreshToken = tokenService.CreateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays());
        var replacement = UserSession.Create(
            user.Id, tokenService.HashRefreshToken(newRefreshToken), metadata.DeviceName,
            metadata.UserAgent, metadata.IpAddress, refreshExpiresAtUtc, current.FamilyId);

        current.Touch(metadata.IpAddress);
        current.Rotate(replacement.Id);
        await unitOfWork.UserSessionRepository.CreateAsync(replacement, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return BuildResponse(user, replacement.FamilyId, newRefreshToken, refreshExpiresAtUtc);
    }

    public async Task RevokeByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var session = await unitOfWork.UserSessionRepository.GetByTokenHashAsync(tokenService.HashRefreshToken(refreshToken), cancellationToken);
        if (session is null) return;
        await unitOfWork.UserSessionRepository.RevokeFamilyAsync(session.UserId, session.FamilyId, "Logout.", cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private LoginResponse BuildResponse(AppUser user, Guid sessionId, string refreshToken, DateTime refreshExpiresAtUtc)
    {
        var accessToken = tokenService.CreateTokenUser(user, sessionId);
        return new LoginResponse(
            user.Id, user.Email, user.Name, accessToken.Token, accessToken.DateExpiration,
            storageService.GetPublicUrl(user.ProfilePhotoKey))
        {
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc
        };
    }

    private int GetRefreshTokenLifetimeDays()
        => int.TryParse(configuration["TokenConfiguration:RefreshTokenExpireDays"], out var days) ? Math.Clamp(days, 1, 90) : 30;

    private static Error InvalidSession() => Error.Unauthorized("Auth.InvalidSession", "Sessão inválida ou expirada.");
}
