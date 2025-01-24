using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Races.Commands;

public sealed record CreateRaceCommand : IRequest<Race>
{
    public string Name { get; set; } = null!;
    public string Origin { get; set; } = "New York";
    public string Destination { get; set; } = "Tbilisi";

    public DateTimeOffset TakeOff { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset Landing { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CreateRaceValidator : AbstractValidator<CreateRaceCommand>
{
    public CreateRaceValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.Name).MaximumLength(32);
        RuleFor(x => x.Origin).MaximumLength(16);

        RuleFor(x => x.Destination).MaximumLength(16);
        RuleFor(x => x.TakeOff)
            .Must(date => date >= DateTimeOffset.UtcNow.Date)
            .WithMessage("Takeoff must be in the future");

        RuleFor(x => x.Landing)
            .Must(date => date >= DateTimeOffset.UtcNow.Date)
            .WithMessage("Landing date must be in the future")
            .Must((command, date) => date > command.TakeOff)
            .WithMessage("Landing date date must be after takeoff time");

        RuleSet("async", () =>
        {
            RuleFor(x => x.Name)
                .MustAsync(async (_, name, ct) =>
                {
                    var flightsWithName = await dbContext.Races.CountAsync(x => x.Name == name, ct);
                    return flightsWithName == 0;
                })
                .WithMessage(command => $"Flight with name {command.Name} already exists!");
        });
    }
}

internal sealed class CreateRaceHandler(IAppDbContext dbContext) : IRequestHandler<CreateRaceCommand, Race>
{
    public async Task<Race> Handle(CreateRaceCommand request, CancellationToken ct)
    {
        var race = new Race(RaceId.New())
        {
            Name = request.Name,
            Origin = request.Origin,
            Destination = request.Destination,
            Start = request.TakeOff.ToUniversalTime().Date,
            Arrival = request.Landing.ToUniversalTime().Date,
            Packages = [],
        };

        dbContext.Races.Add(race);
        await dbContext.SaveChangesAsync(ct);

        return race;
    }
}