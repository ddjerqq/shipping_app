using System.Text.Json;
using Application.Services;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Cqrs.Staff.Queries;

public sealed record MonthlyAdminStats(
    int PackagesThisMonth,
    int PackagesLastMonth,
    float ShippingRevenueThisMonth,
    float ShippingRevenueLastMonth,
    int TotalRaces,
    int RacesThisMonth,
    int RacesLastMonth,
    int TotalUsers,
    int UsersThisMonth,
    int UsersLastMonth)
{
    public float MonthlyPackageChange => (PackagesThisMonth - PackagesLastMonth) / ((float)PackagesLastMonth == 0 ? 1 : (float)PackagesLastMonth) * 100;
    public float MonthlyRevenueChange => (ShippingRevenueThisMonth - ShippingRevenueLastMonth) / (ShippingRevenueLastMonth == 0 ? 1 : ShippingRevenueLastMonth) * 100;
    public float MonthlyRaceChange => (RacesThisMonth - RacesLastMonth) / ((float)RacesLastMonth == 0 ? 1 : (float)RacesLastMonth) * 100;
    public float MonthlyUserChange => (UsersThisMonth - UsersLastMonth) / ((float)UsersLastMonth == 0 ? 1 : (float)UsersLastMonth) * 100;
}

public sealed record FetchMonthlyAdminStatsQuery : IRequest<MonthlyAdminStats>;

internal sealed class FetchMonthlyAdminStatsQueryHandler(IAppDbContext dbContext, IDistributedCache cache) : IRequestHandler<FetchMonthlyAdminStatsQuery, MonthlyAdminStats>
{
    private const string CacheKey = nameof(FetchMonthlyAdminStatsQueryHandler);

    private static readonly int NowYear = DateTime.UtcNow.Year;
    private static readonly int NowMonth = DateTime.UtcNow.Month;

    private static readonly int LastYear = NowMonth == 1 ? NowYear - 1 : NowYear;
    private static readonly int LastMonth = NowMonth == 1 ? 12 : NowMonth - 1;

    private async Task<(int ThisMonth, int LastMonth)> GetMonthlyPackageCounts(CancellationToken ct)
    {
        var packages = await dbContext.Packages
            .Where(p =>
                (p.Created.Year == NowYear && p.Created.Month == NowMonth) ||
                (p.Created.Year == LastYear && p.Created.Month == LastMonth))
            .Select(p => p.Created)
            .ToListAsync(ct);

        var packagesThisMonth = packages.Count(packageDate => packageDate.Year == NowYear && packageDate.Month == NowMonth);
        var packagesLastMonth = packages.Count(packageDate => packageDate.Year == LastYear && packageDate.Month == LastMonth);

        return (packagesThisMonth, packagesLastMonth);
    }

    private async Task<(decimal ThisMonth, decimal LastMonth)> GetPackageShippingRevenuesSums(CancellationToken ct)
    {
        var packageData = await dbContext.Packages
            .Where(p =>
                p.Weight != null && p.Dimensions != null &&
                ((p.Created.Year == NowYear && p.Created.Month == NowMonth) ||
                 (p.Created.Year == LastYear && p.Created.Month == LastMonth))
            )
            .Select(p => new
            {
                p.Created,
                p.Dimensions,
                p.Weight,
                p.HouseDelivery,
            })
            .ToListAsync(ct);

        var shippingRevenueThisMonth = packageData
            .Where(p => p.Created.Year == NowYear && p.Created.Month == NowMonth)
            .Sum(p => PackagePrice.GetTotalPrice(p.Dimensions!.Value.X, p.Dimensions.Value.Y, p.Dimensions.Value.Z, p.Weight!.Value, p.HouseDelivery).Amount);

        var shippingRevenueLastMonth = packageData
            .Where(p => p.Created.Year == LastYear && p.Created.Month == LastMonth)
            .Sum(p => PackagePrice.GetTotalPrice(p.Dimensions!.Value.X, p.Dimensions.Value.Y, p.Dimensions.Value.Z, p.Weight!.Value, p.HouseDelivery).Amount);

        return (shippingRevenueThisMonth, shippingRevenueLastMonth);
    }

    private async Task<(int Total, int ThisMonth, int LastMont)> GetRaceCounts(CancellationToken ct)
    {
        var totalRaces = await dbContext.Races.CountAsync(ct);
        var raceDates = await dbContext.Races
            .Where(r =>
                (r.Created.Year == NowYear && r.Created.Month == NowMonth) ||
                (r.Created.Year == LastYear && r.Created.Month == LastMonth))
            .Select(r => r.Created)
            .ToListAsync(ct);

        var racesThisMonth = raceDates.Count(raceDate => raceDate.Year == NowYear && raceDate.Month == NowMonth);
        var racesLastMonth = raceDates.Count(raceDate => raceDate.Year == LastYear && raceDate.Month == LastMonth);

        return (totalRaces, racesThisMonth, racesLastMonth);
    }

    private async Task<(int Total, int ThisMonth, int LastMonth)> GetUserCounts(CancellationToken ct)
    {
        var totalUsers = await dbContext.Users.CountAsync(ct);

        var users = await dbContext.Users
            .Where(u =>
                (u.Created.Year == NowYear && u.Created.Month == NowMonth) ||
                (u.Created.Year == LastYear && u.Created.Month == LastMonth))
            .ToListAsync(ct);

        var usersThisMonth = users.Count(u => u.Created.Year == NowYear && u.Created.Month == NowMonth);
        var usersLastMonth = users.Count(u => u.Created.Year == LastYear && u.Created.Month == LastMonth);

        return (totalUsers, usersThisMonth, usersLastMonth);
    }

    public async Task<MonthlyAdminStats> Handle(FetchMonthlyAdminStatsQuery request, CancellationToken ct)
    {
        var value = await cache.GetStringAsync(CacheKey, ct);
        if (value is not null)
            return JsonSerializer.Deserialize<MonthlyAdminStats>(value)!;

        var (packagesThisMonth, packagesLastMonth) = await GetMonthlyPackageCounts(ct);
        var (shippingRevenueThisMonth, shippingRevenueLastMonth) = await GetPackageShippingRevenuesSums(ct);
        var (totalRaces, racesThisMonth, racesLastMonth) = await GetRaceCounts(ct);
        var (totalUsers, usersThisMonth, usersLastMonth) = await GetUserCounts(ct);

        var stats = new MonthlyAdminStats(
            packagesThisMonth,
            packagesLastMonth,
            (float)shippingRevenueThisMonth,
            (float)shippingRevenueLastMonth,
            totalRaces,
            racesThisMonth,
            racesLastMonth,
            totalUsers,
            usersThisMonth,
            usersLastMonth);

        var payload = JsonSerializer.Serialize(stats);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        };
        await cache.SetStringAsync(CacheKey, payload, options, ct);

        return stats;
    }
}