namespace Archdraw.BuildingBlocks.Common.Interfaces.Events;

public interface IHasDomainEvent
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
