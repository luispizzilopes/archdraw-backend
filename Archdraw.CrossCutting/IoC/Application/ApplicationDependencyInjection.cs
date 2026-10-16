using Archdraw.Application.Behaviors;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Application.Modules.Diagrams.Services;
using Archdraw.Application.Modules.Documents.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(IUseCase).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainExceptionBehavior<,>));

        services.AddValidatorsFromAssembly(applicationAssembly);

        foreach (var type in applicationAssembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IUseCase).IsAssignableFrom(t)))
            services.AddScoped(type);

        services.AddScoped<AuthSessionService>();
        services.AddScoped<SnapshotPreparer>();
        services.AddScoped<DocumentOwnerResolver>();
    }
}
