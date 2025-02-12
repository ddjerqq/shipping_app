using Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Commands;

public sealed record FlagPackageAsProhibitedCommand(string TrackingCode) : IRequest;

internal sealed class FlagPackageAsProhibitedCommandHandler(IAppDbContext dbContext) : IRequestHandler<FlagPackageAsProhibitedCommand>
{
    public async Task Handle(FlagPackageAsProhibitedCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages.FirstOrDefaultAsync(x => x.TrackingCode.Value == request.TrackingCode, ct);
        if (package is null)
            throw new InvalidOperationException("Package not found");

        package.FlagAsProhibited();
        await dbContext.SaveChangesAsync(ct);
    }
}