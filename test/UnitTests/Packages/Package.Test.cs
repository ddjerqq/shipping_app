using System.Numerics;
using System.Text.Json;
using Application.JsonConverters;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace UnitTests.Packages;

public sealed class PackageTests
{
    [Test]
    public void Test_Creation()
    {
        var staffOrigin = new User
        {
            Id = UserId.New(),
            UserName = "staff origin",
            Email = "staff@origin.com",
            PersonalId = "01001000000",
        };

        var staffDestination = new User
        {
            Id = UserId.New(),
            UserName = "staff destination",
            Email = "staff@destination.com",
            PersonalId = "01001000000",
        };

        var sender = new User
        {
            Id = UserId.New(),
            UserName = "sender",
            Email = "sender@gmail.com",
            PersonalId = "01001000000",
        };

        var originTime = new DateTimeOffset(2024, 11, 11, 0, 0, 0, TimeSpan.FromHours(-5));
        var destTime = new DateTimeOffset(2024, 11, 11, 0, 0, 0, TimeSpan.FromHours(4));

        var race = new Race(RaceId.New())
        {
            Name = "KAL82-KE82",
            Origin = "JFK",
            Start = originTime,
            Destination = "TBS",
            Arrival = destTime.AddHours(8),
        };

        var package = Package.Create(
            "JFK",
            "TBS",
            null,
            Category.Toy,
            "water blaster toy for kids",
            new WebAddress("amazon.com"),
            new Money("USD",20),
            false,
            sender);

        package.ArrivedAtOrigin(staffOrigin, new Vector3(100, 100, 100), 100, originTime);
        package.SentToDestination(staffOrigin, race, originTime.AddHours(4));
        package.ArrivedAtDestination(staffDestination, destTime.AddHours(12));
        package.Delivered(destTime.AddHours(14));

        var json = JsonSerializer.Serialize(package, ApplicationJsonConstants.Options.Value);
        Console.WriteLine(json);
    }
}