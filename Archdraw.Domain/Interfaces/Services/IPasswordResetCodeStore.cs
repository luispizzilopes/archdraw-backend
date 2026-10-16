namespace Archdraw.Domain.Interfaces.Services;

public interface IPasswordResetCodeStore
{
    Task StoreAsync(string email, string code, CancellationToken cancellationToken = default);
    Task<string?> GetAsync(string email, CancellationToken cancellationToken = default);
    Task<int> RegisterFailedAttemptAsync(string email, CancellationToken cancellationToken = default);
    Task RemoveAsync(string email, CancellationToken cancellationToken = default);
}
