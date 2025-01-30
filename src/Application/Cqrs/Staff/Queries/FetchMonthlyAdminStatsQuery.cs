using Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Staff.Queries;

public sealed record MonthlyAdminStats(
    int PackagesThisMonth,
    int PackagesLastMonth,
    float TotalRevenueThisMonth,
    float TotalRevenueLastMonth,
    int TotalPackagesThisMonth,
    int TotalPackagesLastMonth,
    int TotalUsersThisMonth,
    int TotalUsersLastMonth)
{
    public float MonthlyPackageChangePercentage => (PackagesThisMonth - PackagesLastMonth) / (float)PackagesLastMonth * 100;
    public float MonthlyRevenueChangePercentage => (TotalRevenueThisMonth - TotalRevenueLastMonth) / TotalRevenueLastMonth * 100;
    public float MonthlyPackageCountChangePercentage => (TotalPackagesThisMonth - TotalPackagesLastMonth) / (float)TotalPackagesLastMonth * 100;
    public float MonthlyUserCountChangePercentage => (TotalUsersThisMonth - TotalUsersLastMonth) / (float)TotalUsersLastMonth * 100;
}

public sealed record FetchMonthlyAdminStatsQuery: IRequest<MonthlyAdminStats>;

internal sealed class FetchMonthlyAdminStatsQueryHandler(IAppDbContext dbContext) : IRequestHandler<FetchMonthlyAdminStatsQuery, MonthlyAdminStats>
{
    public async Task<MonthlyAdminStats> Handle(FetchMonthlyAdminStatsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nowYear = now.Year;
        var nowMonth = now.Month;

        var lastYear = nowMonth == 1 ? nowYear - 1 : nowYear;
        var lastMonth = nowMonth == 1 ? 12 : nowMonth - 1;

        var packagesThisMonth = await dbContext.Packages
            .Where(p => p.Created.Year >= nowYear && p.Created.Month == nowMonth)
            .CountAsync(ct);

        var packagesLastMonth = await dbContext.Packages
            .Where(p => p.Created.Year >= lastYear && p.Created.Month == lastMonth)
            .CountAsync(ct);

        var totalRevenueThisMonth = dbContext.Packages
            .Where(p => p.Created.Year >= nowYear && p.Created.Month == nowMonth)
            .AsEnumerable()
            .Sum(p => p.ShippingPrice);

        var totalRevenueLastMonth = dbContext.Packages
            .Where(p => p.Created.Year >= lastYear && p.Created.Month == lastMonth)
            .AsEnumerable()
            .Sum(p => p.ShippingPrice);

        var totalPackagesThisMonth = await dbContext.Packages
            .Where(p => p.Created.Year >= nowYear && p.Created.Month == nowMonth)
            .CountAsync(ct);

        var totalPackagesLastMonth = await dbContext.Packages
            .Where(p => p.Created.Year >= lastYear && p.Created.Month == lastMonth)
            .CountAsync(ct);

        var totalUsersThisMonth = await dbContext.Users
            .Where(u => u.Created.Year >= nowYear && u.Created.Month == nowMonth)
            .CountAsync(ct);

        var totalUsersLastMonth = await dbContext.Users
            .Where(u => u.Created.Year >= lastYear && u.Created.Month == lastMonth)
            .CountAsync(ct);

        return new MonthlyAdminStats(
            packagesThisMonth,
            packagesLastMonth,
            (float)totalRevenueThisMonth,
            (float)totalRevenueLastMonth,
            totalPackagesThisMonth,
            totalPackagesLastMonth,
            totalUsersThisMonth,
            totalUsersLastMonth);
    }
}