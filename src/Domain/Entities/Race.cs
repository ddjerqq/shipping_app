using Domain.Abstractions;
using Domain.Aggregates;
using Domain.ValueObjects;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class Race(RaceId id) : Entity<RaceId>(id)
{
    public required string Name { get; init; }
    public required AbstractAddress Origin { get; init; }
    public required AbstractAddress Destination { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset Arrival { get; init; }

    public ICollection<Package> Packages { get; init; } = [];
}