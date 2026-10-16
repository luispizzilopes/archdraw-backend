using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.Services;

namespace Archdraw.Application.Modules.Auth.UseCases.RefreshSession;

public class RefreshSessionUseCase(AuthSessionService authSessionService) : IUseCase
{
    public Task<ErrorOr<LoginResponse>> ExecuteAsync(string refreshToken, SessionMetadata metadata, CancellationToken cancellationToken = default)
        => authSessionService.RefreshAsync(refreshToken, metadata, cancellationToken);
}
