// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using Domain.Abstractions;
using Generated;
using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates;

[StrongId]
public sealed class User : IdentityUser<UserId>, IAggregateRoot<UserId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public string PersonalId { get; init; }

    public string AvatarUrl { get; init; }

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}