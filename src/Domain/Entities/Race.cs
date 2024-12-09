using Domain.Abstractions;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class Race(RaceId id) : Entity<RaceId>(id)
{
    public required string Name { get; init; }
    public required string Origin { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required string Destination { get; init; }
    public required DateTimeOffset Arrival { get; init; }

    public ICollection<Package> Packages { get; init; } = [];
}