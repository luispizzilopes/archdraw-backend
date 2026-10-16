using Archdraw.Domain.Interfaces.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Archdraw.BackgroundJobs.Jobs;

public class ExpiredSessionsCleanupJob(IServiceScopeFactory scopeFactory, ILogger<ExpiredSessionsCleanupJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await uow.UserSessionRepository.DeleteExpiredAsync(DateTime.UtcNow.AddDays(-30), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao limpar sessões expiradas.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
