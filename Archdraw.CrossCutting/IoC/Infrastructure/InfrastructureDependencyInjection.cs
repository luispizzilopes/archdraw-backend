using Archdraw.CrossCutting.IoC.Infrastructure.Context;
using Archdraw.CrossCutting.IoC.Infrastructure.Identity;
using Archdraw.CrossCutting.IoC.Infrastructure.Services;
using Archdraw.CrossCutting.IoC.Infrastructure.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static void AddInfrastructureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppDbContextDependencyInjection(configuration);
        services.AddIdentityDependencyInjection(configuration);
        services.AddUnitOfWorkDependencyInjection();
        services.AddServicesDependencyInjection(configuration);
    }
}
