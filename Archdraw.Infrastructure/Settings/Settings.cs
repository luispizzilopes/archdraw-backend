namespace Archdraw.Infrastructure.Settings;

public class BrevoSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Archdraw";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(FromEmail);
}

public class AwsSettings
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Region { get; set; } = "auto";
    public string EndpointUrl { get; set; } = string.Empty;
    public string PublicBucketName { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(AccessKeyId) && !string.IsNullOrWhiteSpace(SecretAccessKey)
                                && !string.IsNullOrWhiteSpace(PublicBucketName) && !string.IsNullOrWhiteSpace(PublicBaseUrl);
}

public class LocalStorageSettings
{
    public string RootPath { get; set; } = "uploads";
    public string PublicBaseUrl { get; set; } = "http://localhost:5000/uploads";
}
