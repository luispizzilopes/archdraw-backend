using Archdraw.Domain.Modules.Users.Entities;

namespace Archdraw.Domain.Interfaces.Repositories.Users;

public interface IUserSessionRepository
{
    Task CreateAsync(UserSession session, CancellationToken ct = default);
    Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<UserSession?> GetByFamilyIdAsync(Guid familyId, Guid userId, CancellationToken ct = default);
    Task<bool> IsActiveAsync(Guid familyId, Guid userId, CancellationToken ct = default);
    Task RevokeAllAsync(Guid userId, string reason, Guid? exceptFamilyId, CancellationToken ct = default);
    Task RevokeFamilyAsync(Guid userId, Guid familyId, string reason, CancellationToken ct = default);
    Task DeleteExpiredAsync(DateTime olderThanUtc, CancellationToken ct = default);
}
