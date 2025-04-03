using System.Globalization;
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

public sealed record RegisterCommand : IRequest<User>
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
    public string Password { get; set; } = null!;

    [LogMasked]
    public TimeZoneInfo TimeZoneInfo { get; set; } = null!;

    [LogMasked]
    public CultureInfo CultureInfo { get; set; } = null!;

    public bool AgreeToTerms { get; set; }
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(5).MaximumLength(32);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"\d{3}\d{9}").WithMessage("Please enter international standard phone number (eg. 995599123123)");
        RuleFor(x => x.PersonalId).NotEmpty().Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");
        RuleFor(x => x.AgreeToTerms).Equal(true).WithMessage("You must agree to terms in order to register.");

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

internal sealed class RegisterCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<RegisterCommand, User>
{
    public async Task<User> Handle(RegisterCommand request, CancellationToken ct)
    {
        await using var transaction = await dbContext.BeginTransactionAsync(ct);

        var user = new User(UserId.New())
        {
            PersonalId = request.PersonalId,
            Username = request.FullName.ToLowerInvariant(),
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            AddressInfo = new NoAddress(),
            CultureInfo = request.CultureInfo,
            TimeZone = request.TimeZoneInfo,
        };
        user.SetPassword(request.Password, true);
        user.AddDomainEvent(new UserRegistered(user.Id));

        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return user;
    }
}