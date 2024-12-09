using Domain.Aggregates;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class Role : IdentityRole<UserId> { }