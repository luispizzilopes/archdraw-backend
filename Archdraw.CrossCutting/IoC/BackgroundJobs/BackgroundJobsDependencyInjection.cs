using Archdraw.BackgroundJobs.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.BackgroundJobs;

public static class BackgroundJobsDependencyInjection
{
    public static void AddBackgroundJobsDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("BackgroundJobs:Enabled"))
            services.AddHostedService<ExpiredSessionsCleanupJob>();
    }
}
