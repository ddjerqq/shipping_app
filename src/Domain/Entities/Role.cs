using Domain.Abstractions;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class Role(RoleId id) : Entity<RoleId>(id)
{
    public required string Name { get; init; }
    public string ConcurrencyStamp { get; init; } = Guid.NewGuid().ToString();

    public ICollection<RoleClaim> Claims { get; init; } = [];
}