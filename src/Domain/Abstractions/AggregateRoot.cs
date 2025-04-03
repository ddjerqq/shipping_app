using Destructurama.Attributed;
using Generated;

namespace Domain.Abstractions;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id), IAggregateRoot<TId>
    where TId : struct, IStrongId, IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    [NotLogged]
    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    
    public void ClearDomainEvents() => _domainEvents.Clear();
}