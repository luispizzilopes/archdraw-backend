using Archdraw.Infrastructure.Context;
using Archdraw.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.Infrastructure.Context;

public static class AppDbContextDependencyInjection
{
    public static void AddAppDbContextDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DomainEventsPublishInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsPublishInterceptor>());
        });
    }
}
