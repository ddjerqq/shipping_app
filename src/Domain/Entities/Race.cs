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
    public required DateTime Start { get; set; }
    public required DateTime Arrival { get; set; }

    public ICollection<Package> Packages { get; init; } = [];

    public void AddPackage(Package package)
    {
        if (package.CurrentStatus.Status != PackageStatus.InWarehouse)
            throw new InvalidOperationException($"Package must be in warehouse. This package has a status of {package.CurrentStatus.Status}");

        Packages.Add(package);
    }

    public string QualifiedName => $"{Origin} - {Destination} ({Name})";
}