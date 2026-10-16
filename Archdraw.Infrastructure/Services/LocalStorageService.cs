using Archdraw.Domain.Interfaces.Services;
using Archdraw.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Archdraw.Infrastructure.Services;

public class LocalStorageService(IOptions<LocalStorageSettings> settings) : IStorageService
{
    private readonly LocalStorageSettings _settings = settings.Value;

    public async Task UploadPublicAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task DeletePublicAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string? GetPublicUrl(string? key)
        => string.IsNullOrEmpty(key) ? null : $"{_settings.PublicBaseUrl.TrimEnd('/')}/{key}";

    private string ResolvePath(string key)
    {
        var root = Path.GetFullPath(_settings.RootPath);
        var full = Path.GetFullPath(Path.Combine(root, key));
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Chave de armazenamento inválida.");
        return full;
    }
}
