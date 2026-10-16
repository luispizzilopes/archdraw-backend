using Amazon.Runtime;
using Amazon.S3;
using Archdraw.Application.Interfaces.Services;
using Archdraw.Domain.Interfaces.Caching;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Infrastructure.Services;
using Archdraw.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.Infrastructure.Services;

public static class ServicesDependencyInjection
{
    public static void AddServicesDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenJwtService, TokenJwtService>();

        var brevo = configuration.GetSection("BrevoSettings").Get<BrevoSettings>() ?? new BrevoSettings();
        services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));
        if (brevo.IsConfigured)
        {
            services.AddHttpClient("Brevo", client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/v3/");
                client.DefaultRequestHeaders.Add("api-key", brevo.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(15);
            });
            services.AddScoped<IEmailService, BrevoEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, LogEmailService>();
        }

        var aws = configuration.GetSection("AwsSettings").Get<AwsSettings>() ?? new AwsSettings();
        services.Configure<AwsSettings>(configuration.GetSection("AwsSettings"));
        services.Configure<LocalStorageSettings>(configuration.GetSection("LocalStorage"));
        if (aws.IsConfigured)
        {
            services.AddSingleton<IAmazonS3>(_ =>
            {
                var config = new AmazonS3Config { ForcePathStyle = true };
                if (!string.IsNullOrWhiteSpace(aws.EndpointUrl)) config.ServiceURL = aws.EndpointUrl;
                else config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(aws.Region);
                return new AmazonS3Client(new BasicAWSCredentials(aws.AccessKeyId, aws.SecretAccessKey), config);
            });
            services.AddScoped<IStorageService, S3StorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, LocalStorageService>();
        }

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
            services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = redis;
                o.InstanceName = configuration["Redis:InstanceName"] ?? "archdraw:";
            });
        else
            services.AddDistributedMemoryCache();

        services.AddScoped<IPasswordResetCodeStore, PasswordResetCodeStore>();
        services.AddScoped<ICacheService, DistributedCacheService>();
    }
}
