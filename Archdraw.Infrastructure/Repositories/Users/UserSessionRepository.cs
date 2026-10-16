using Archdraw.Domain.Interfaces.Repositories.Users;
using Archdraw.Domain.Modules.Users.Entities;
using Archdraw.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Repositories.Users;

public class UserSessionRepository(AppDbContext context) : IUserSessionRepository
{
    public async Task CreateAsync(UserSession session, CancellationToken ct = default)
        => await context.UserSessions.AddAsync(session, ct);

    public Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => context.UserSessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash, ct);

    public Task<UserSession?> GetByFamilyIdAsync(Guid familyId, Guid userId, CancellationToken ct = default)
        => context.UserSessions.Where(s => s.FamilyId == familyId && s.UserId == userId && s.RevokedAtUtc == null)
            .OrderByDescending(s => s.CreatedAtUtc).FirstOrDefaultAsync(ct);

    public Task<bool> IsActiveAsync(Guid familyId, Guid userId, CancellationToken ct = default)
        => context.UserSessions.AsNoTracking().AnyAsync(
            s => s.FamilyId == familyId && s.UserId == userId && s.RevokedAtUtc == null && s.ExpiresAtUtc > DateTime.UtcNow, ct);

    public async Task RevokeAllAsync(Guid userId, string reason, Guid? exceptFamilyId, CancellationToken ct = default)
    {
        var sessions = await context.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null && (exceptFamilyId == null || s.FamilyId != exceptFamilyId))
            .ToListAsync(ct);
        foreach (var session in sessions) session.Revoke(reason);
    }

    public async Task RevokeFamilyAsync(Guid userId, Guid familyId, string reason, CancellationToken ct = default)
    {
        var sessions = await context.UserSessions
            .Where(s => s.UserId == userId && s.FamilyId == familyId && s.RevokedAtUtc == null).ToListAsync(ct);
        foreach (var session in sessions) session.Revoke(reason);
    }

    public Task DeleteExpiredAsync(DateTime olderThanUtc, CancellationToken ct = default)
        => context.UserSessions.Where(s => s.ExpiresAtUtc < olderThanUtc).ExecuteDeleteAsync(ct);
}
