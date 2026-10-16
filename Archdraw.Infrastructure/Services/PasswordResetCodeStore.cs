using Archdraw.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Archdraw.Infrastructure.Services;

public class PasswordResetCodeStore(IDistributedCache cache) : IPasswordResetCodeStore
{
    private static readonly DistributedCacheEntryOptions Expiry = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

    private static string CodeKey(string email) => $"pwreset:{email.ToLowerInvariant()}";
    private static string AttemptsKey(string email) => $"pwreset-attempts:{email.ToLowerInvariant()}";

    public async Task StoreAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        await cache.SetStringAsync(CodeKey(email), code, Expiry, cancellationToken);
        await cache.RemoveAsync(AttemptsKey(email), cancellationToken);
    }

    public Task<string?> GetAsync(string email, CancellationToken cancellationToken = default)
        => cache.GetStringAsync(CodeKey(email), cancellationToken);

    public async Task<int> RegisterFailedAttemptAsync(string email, CancellationToken cancellationToken = default)
    {
        var current = int.TryParse(await cache.GetStringAsync(AttemptsKey(email), cancellationToken), out var n) ? n : 0;
        current++;
        await cache.SetStringAsync(AttemptsKey(email), current.ToString(), Expiry, cancellationToken);
        return current;
    }

    public async Task RemoveAsync(string email, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(CodeKey(email), cancellationToken);
        await cache.RemoveAsync(AttemptsKey(email), cancellationToken);
    }
}
