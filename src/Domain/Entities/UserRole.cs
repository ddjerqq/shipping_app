using Domain.Aggregates;

namespace Domain.Entities;

public sealed class UserRole
{
    public User User { get; set; } = default!;
    public UserId UserId { get; set; }

    public Role Role { get; set; } = default!;
    public RoleId RoleId { get; set; }
}