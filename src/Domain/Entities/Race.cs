using Domain.Abstractions;
using Domain.Aggregates;
using Domain.ValueObjects;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class Race(RaceId id) : Entity<RaceId>(id)
{
    public required string Name { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public required DateTimeOffset Start { get; set; }
    public required DateTimeOffset Arrival { get; set; }

    public ICollection<Package> Packages { get; init; } = [];

    public string QualifiedName => $"{Origin} - {Destination} ({Name})";
}