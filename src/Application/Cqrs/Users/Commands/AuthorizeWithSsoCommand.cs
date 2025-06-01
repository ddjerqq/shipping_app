using System.Globalization;
using Application.Services;
using Domain.Aggregates;
using Domain.Events;
using Domain.ValueObjects;
using EntityFrameworkCore.DataProtection.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Commands;

public sealed record AuthorizeWithSsoCommand(string Email, string FullName) : IRequest<(string Token, User User)>;

internal sealed class AuthorizeWithSsoCommandHandler(IAppDbContext dbContext, IJwtGenerator jwtGenerator) : IRequestHandler<AuthorizeWithSsoCommand, (string Token, User User)>
{
    public async Task<(string Token, User User)> Handle(AuthorizeWithSsoCommand request, CancellationToken ct = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .WherePdEquals(nameof(User.Email), request.Email.ToLowerInvariant())
            .SingleOrDefaultAsync(ct);

        if (user is null)
        {
            await using var transaction = await dbContext.BeginTransactionAsync(ct);

            user = new User(UserId.New())
            {
                Username = request.FullName.ToLowerInvariant(),
                Email = request.Email.ToLowerInvariant(),
                AddressInfo = new NoAddress(),
                CultureInfo = new CultureInfo("en-US"),
                TimeZone = TimeZoneInfo.Utc,
            };
            user.AddDomainEvent(new UserRegistered(user.Id));

            await dbContext.Users.AddAsync(user, ct);
            await dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }

        var token = jwtGenerator.GenerateToken(user);
        return (token, user);
    }
}