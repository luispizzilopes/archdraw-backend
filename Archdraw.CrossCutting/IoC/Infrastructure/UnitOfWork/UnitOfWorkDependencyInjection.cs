using Archdraw.Domain.Interfaces.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.CrossCutting.IoC.Infrastructure.UnitOfWork;

public static class UnitOfWorkDependencyInjection
{
    public static void AddUnitOfWorkDependencyInjection(this IServiceCollection services)
        => services.AddScoped<IUnitOfWork, global::Archdraw.Infrastructure.UnitOfWork.UnitOfWork>();
}
