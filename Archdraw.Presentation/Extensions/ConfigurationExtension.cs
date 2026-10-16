using Archdraw.CrossCutting.IoC.Application;
using Archdraw.CrossCutting.IoC.BackgroundJobs;
using Archdraw.CrossCutting.IoC.Infrastructure;
using Archdraw.Presentation.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace Archdraw.Presentation.Extensions;

public static class ConfigurationExtension
{
    public static void AddConfigurations(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        if (environment.IsProduction() && string.IsNullOrWhiteSpace(configuration["Swagger:Password"]))
            throw new InvalidOperationException("Swagger:Password deve ser configurada em Production.");

        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(x => new ApiErrorDetail(e.Key, string.IsNullOrEmpty(x.ErrorMessage) ? "Valor inválido." : x.ErrorMessage)))
                    .ToList();
                return new BadRequestObjectResult(new ApiError("Request.Invalid", "Requisição inválida.", "Validation", errors, null));
            };
        });

        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(o => o.Limits.MaxRequestBodySize = 10 * 1024 * 1024);
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration(configuration);
        services.AddRateLimitConfiguration(configuration);

        services.AddInfrastructureDependencyInjection(configuration);
        services.AddApplicationDependencyInjection(configuration);
        services.AddBackgroundJobsDependencyInjection(configuration);

        services.AddCors();
    }
}
