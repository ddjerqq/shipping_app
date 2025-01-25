using System.Globalization;
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
        var staffOrigin = new User(UserId.New())
        {
            Id = UserId.New(),
            Username = "staff origin",
            Email = "staff@origin.com",
            PhoneNumber = "",
            PersonalId = "01001000000",
            AddressInfo = new NoAddress(),
            TimeZone = TimeZoneInfo.Local,
            CultureInfo = CultureInfo.CurrentCulture,
        };

        var staffDestination = new User(UserId.New())
        {
            Id = UserId.New(),
            Username = "staff destination",
            Email = "staff@destination.com",
            PhoneNumber = "",
            PersonalId = "01001000000",
            AddressInfo = new NoAddress(),
            TimeZone = TimeZoneInfo.Local,
            CultureInfo = CultureInfo.CurrentCulture,
        };

        var sender = new User(UserId.New())
        {
            Id = UserId.New(),
            Username = "sender",
            Email = "sender@gmail.com",
            PhoneNumber = "",
            PersonalId = "01001000000",
            AddressInfo = new NoAddress(),
            TimeZone = TimeZoneInfo.Local,
            CultureInfo = CultureInfo.CurrentCulture,
        };

        var originTime = new DateTime(2024, 11, 11, 0, 0, 0, DateTimeKind.Utc);
        var destTime = new DateTime(2024, 11, 11, 0, 0, 0, DateTimeKind.Utc);

        var race = new Race(RaceId.New())
        {
            Name = "KAL82-KE82",
            Origin = "JFK",
            Start = originTime,
            Destination = "TBS",
            Arrival = destTime.AddHours(8),
        };

        var package = Package.Create(
            null,
            Category.Toy,
            "water blaster toy for kids",
            "amazon.com",
            new Money("USD", 20),
            1,
            false,
            sender);

        package.ArrivedAtWarehouse(staffOrigin, new Vector3(100, 100, 100), 100, originTime);
        package.SentToDestination(staffOrigin, race, originTime.AddHours(4));
        package.ArrivedAtDestination(staffDestination, destTime.AddHours(12));
        package.Delivered(destTime.AddHours(14));

        var json = JsonSerializer.Serialize(package, ApplicationJsonConstants.Options.Value);
        Console.WriteLine(json);
    }
}