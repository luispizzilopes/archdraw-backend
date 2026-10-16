namespace Archdraw.Domain.Interfaces.Services;

public interface IStorageService
{
    Task UploadPublicAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeletePublicAsync(string key, CancellationToken cancellationToken = default);
    string? GetPublicUrl(string? key);
}
