using Archdraw.BuildingBlocks.Common.Interfaces.Events;

namespace Archdraw.BuildingBlocks.Common.Class.Base;

public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvent
    where TId : notnull
{
    public TId Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity() { Id = default!; }

    protected Entity(TId id) { Id = id; }

    public void AddEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Id.Equals(entity.Id);

    public bool Equals(Entity<TId>? other) => Equals((object?)other);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);

    public override int GetHashCode() => Id.GetHashCode();
}
