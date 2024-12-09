using Domain.Aggregates;
using Microsoft.AspNetCore.Identity;

namespace Domain.Abstractions;

public abstract class ApplicationUser : IdentityUser<UserId>, IAggregateRoot<UserId>
{
    protected readonly List<IDomainEvent> Events = [];

    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? Deleted { get; set; }
    public string? DeletedBy { get; set; }

    public IEnumerable<IDomainEvent> DomainEvents => Events;

    public void AddDomainEvent(IDomainEvent domainEvent) => Events.Add(domainEvent);

    public void ClearDomainEvents() => Events.Clear();
}