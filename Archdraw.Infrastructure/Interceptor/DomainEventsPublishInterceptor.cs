using Archdraw.BuildingBlocks.Common.Interfaces.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Archdraw.Infrastructure.Interceptor;

public class DomainEventsPublishInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventsPublishInterceptor(IPublisher publisher) => _publisher = publisher;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEvents(DbContext? dbContext, CancellationToken cancellationToken)
    {
        if (dbContext is null) return;

        var entitiesWithEvents = dbContext.ChangeTracker.Entries<IHasDomainEvent>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
    }
}
