using System.Globalization;
using System.Security.Cryptography;
using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using Domain.Events;
using Domain.ValueObjects;
using EntityFrameworkCore.DataProtection.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Commands;

public sealed record CreateUserCommand : IRequest<User>
{
    [LogMasked]
    public string FullName { get; set; } = null!;

    [LogMasked]
    public string Email { get; set; } = null!;

    [LogMasked]
    public string PhoneNumber { get; set; } = null!;

    [LogMasked]
    public string PersonalId { get; set; } = null!;

    [LogMasked]
    public string Password { get; set; } = RandomNumberGenerator.GetHexString(12, true);

    [LogMasked]
    public Role Role { get; set; } = Role.User;

    [LogMasked]
    public AbstractAddress AddressInfo { get; set; } = new NoAddress();

    [LogMasked]
    public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

    [LogMasked]
    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Utc;
}

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(5).MaximumLength(32);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"\d{3}\d{9}").WithMessage("Please enter international standard phone number (eg. 995599123123)");
        RuleFor(x => x.PersonalId).NotEmpty().Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");

        RuleSet("async", () =>
        {
            RuleFor(x => x.Email)
                .MustAsync(async (_, email, ct) =>
                {
                    var usersWithPd = await dbContext.Users.WherePdEquals(nameof(User.Email), email.ToLowerInvariant()).CountAsync(ct);
                    return usersWithPd == 0;
                }).WithMessage("User with {PropertyName} already exists");

            RuleFor(x => x.PhoneNumber)
                .MustAsync(async (_, phoneNumber, ct) =>
                {
                    var usersWithPd = await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), phoneNumber).CountAsync(ct);
                    return usersWithPd == 0;
                }).WithMessage("User with {PropertyName} already exists");

            RuleFor(x => x.PersonalId)
                .MustAsync(async (_, personalId, ct) =>
                {
                    var usersWithPd = await dbContext.Users.WherePdEquals(nameof(User.PersonalId), personalId).CountAsync(ct);
                    return usersWithPd == 0;
                }).WithMessage("User with {PropertyName} already exists");
        });
    }
}

internal sealed class CreateUserCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken ct)
    {
        await using var transaction = await dbContext.BeginTransactionAsync(ct);

        var user = new User(UserId.New())
        {
            PersonalId = request.PersonalId,
            Username = request.FullName.ToLowerInvariant(),
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            AddressInfo = request.AddressInfo,
            CultureInfo = request.CultureInfo,
            TimeZone = request.TimeZoneInfo,
            Role = request.Role,
        };
        user.SetPassword(request.Password, true);
        user.ConfirmEmail();
        user.AddDomainEvent(new UserAddedByAdmin(user.Id, request.Password));

        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return user;
    }
}