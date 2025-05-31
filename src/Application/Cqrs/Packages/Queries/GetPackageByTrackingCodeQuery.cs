using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Queries;

public sealed record GetPackageByTrackingCodeQuery : IRequest<Package?>
{
    public string TrackingCode { get; set; } = null!;
}

public sealed class GetPackageByTrackingCodeValidator : AbstractValidator<GetPackageByTrackingCodeQuery>
{
    public GetPackageByTrackingCodeValidator()
    {
        RuleFor(x => x.TrackingCode)
            .NotNull()
            .MinimumLength(10).MaximumLength(64)
            .Must(TrackingCode.IsValid).WithMessage("Tracking code is malformed!");
    }
}

internal sealed class GetPackageByTrackingCodeQueryHandler(ICurrentUserAccessor currentUser, IAppDbContext dbContext) : IRequestHandler<GetPackageByTrackingCodeQuery, Package?>
{
    public async Task<Package?> Handle(GetPackageByTrackingCodeQuery request, CancellationToken ct)
    {
        return await dbContext.Packages
            .FirstOrDefaultAsync(x => x.TrackingCode.Value == request.TrackingCode, ct);
    }
}