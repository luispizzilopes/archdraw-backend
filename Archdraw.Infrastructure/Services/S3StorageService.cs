using Amazon.S3;
using Amazon.S3.Model;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Archdraw.Infrastructure.Services;

public class S3StorageService(IAmazonS3 client, IOptions<AwsSettings> settings) : IStorageService
{
    private readonly AwsSettings _settings = settings.Value;

    public async Task UploadPublicAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        => await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _settings.PublicBucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
            CannedACL = S3CannedACL.PublicRead
        }, cancellationToken);

    public async Task DeletePublicAsync(string key, CancellationToken cancellationToken = default)
        => await client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _settings.PublicBucketName, Key = key }, cancellationToken);

    public string? GetPublicUrl(string? key)
        => string.IsNullOrEmpty(key) ? null : $"{_settings.PublicBaseUrl.TrimEnd('/')}/{key}";
}
