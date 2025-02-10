using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Staff.Queries;

public abstract record GenericSearchResult
{
    public sealed record RaceSearchResult(Race Value) : GenericSearchResult;

    public sealed record PackageSearchResult(Package Value) : GenericSearchResult;

    public sealed record UserSearchResult(User Value) : GenericSearchResult;
}

public sealed record GenericSearchQuery([property: LogMasked] string Value) : IRequest<IEnumerable<GenericSearchResult>>;

internal sealed class GenericSearchQueryHandler(IAppDbContext dbContext) : IRequestHandler<GenericSearchQuery, IEnumerable<GenericSearchResult>>
{
    public async Task<IEnumerable<GenericSearchResult>> Handle(GenericSearchQuery request, CancellationToken ct)
    {
        var query = request.Value.ToLowerInvariant().Trim();

        IEnumerable<GenericSearchResult> races = await dbContext.Races
            .IgnoreAutoIncludes()
            .Where(race => race.Name.Contains(query.ToUpperInvariant()))
            .Select(race => new GenericSearchResult.RaceSearchResult(race))
            .Take(20)
            .ToListAsync(ct);

        var packages = await dbContext.Packages
            .IgnoreAutoIncludes()
            .Include(package => package.Owner)
            .Where(package => package.TrackingCode.Value.Contains(query))
            .Select(package => new GenericSearchResult.PackageSearchResult(package))
            .Take(20)
            .ToListAsync(ct);

        var roomCode = int.TryParse(query, out var x) ? x : 0;
        var hash = query.ToLowerInvariant().HmacSha256Hash();

        IEnumerable<GenericSearchResult> users = await dbContext.Users
            .IgnoreAutoIncludes()
            .Where(user =>
                EF.Property<string>(user, $"{nameof(User.Username)}ShadowHash") == hash ||
                EF.Property<string>(user, $"{nameof(User.Email)}ShadowHash") == hash ||
                EF.Property<string>(user, $"{nameof(User.PhoneNumber)}ShadowHash") == hash ||
                EF.Property<string>(user, $"{nameof(User.PersonalId)}ShadowHash") == hash ||
                user.RoomCode == roomCode)
            .Select(user => new GenericSearchResult.UserSearchResult(user))
            .Take(20)
            .ToListAsync(ct);

        return races.Concat(packages).Concat(users);
    }
}