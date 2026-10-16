using System.Threading.RateLimiting;

namespace Archdraw.Presentation.Extensions;

public static class RateLimitExtension
{
    public const string AuthPolicy = "auth-anonymous";

    public const string RefreshPolicy = "auth-refresh";

    public static IServiceCollection AddRateLimitConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var permits = configuration.GetValue("RateLimit:AuthPermitPerMinute", 10);
        var refreshPermits = configuration.GetValue("RateLimit:RefreshPermitPerMinute", 60);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy(RefreshPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = refreshPermits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        return services;
    }
}
