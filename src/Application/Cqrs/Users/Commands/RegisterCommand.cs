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
using Microsoft.Extensions.Logging;
using RegisterResult = (string Token, Domain.Aggregates.User User)?;

namespace Application.Cqrs.Users.Commands;

public sealed record RegisterCommand : IRequest<RegisterResult>
{
    [LogMasked]
    public string FullName { get; set; } = default!;

    [LogMasked]
    public string Email { get; set; } = default!;

    [LogMasked]
    public string PhoneNumber { get; set; } = default!;

    [LogMasked]
    public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [LogMasked]
    public string PersonalId { get; set; } = default!;

    [LogMasked]
    public string Password { get; set; } = default!;

    [LogMasked]
    public string Country { get; set; } = default!;

    [LogMasked]
    public string? State { get; set; }

    [LogMasked]
    public string City { get; set; } = default!;

    [LogMasked]
    public string ZipCode { get; set; } = default!;

    [LogMasked]
    public string Address { get; set; } = default!;

    [LogMasked]
    public TimeZoneInfo TimeZoneInfo { get; set; } = default!;

    [LogMasked]
    public CultureInfo CultureInfo { get; set; } = default!;
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).MinimumLength(5).MaximumLength(32);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MinimumLength(10).MaximumLength(15);
        RuleFor(x => x.BirthDate).LessThan(DateOnly.FromDateTime(DateTime.Now).AddYears(-18)).WithMessage("You must be 18 years old or older to use this service");
        RuleFor(x => x.PersonalId).Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");
        RuleFor(x => x.Country).Matches("(GEO|US)").WithMessage("Must be GEO or USA");
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.ZipCode).Matches(@"^\d+").WithMessage("Must be digits only");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(256);
    }
}

internal sealed class RegisterCommandHandler(ILogger<RegisterCommandHandler> logger, ISender sender, IAppDbContext dbContext)
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var usersWithEmail = await dbContext.Users.WherePdEquals(nameof(User.Email), request.Email.ToLowerInvariant()).CountAsync(ct);
        var usersWithPhoneId = await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), request.PhoneNumber).CountAsync(ct);
        var usersWithPersonalId = await dbContext.Users.WherePdEquals(nameof(User.PersonalId), request.PersonalId).CountAsync(ct);

        if (usersWithEmail + usersWithPhoneId + usersWithPersonalId > 0)
        {
            logger.LogWarning("User already registered email: {Email}, phone: {PhoneNumber}, personalId: {PersonalId}", request.Email, request.PhoneNumber, request.PersonalId);
            return null;
        }

        await using var transaction = await dbContext.BeginTransactionAsync(ct);

        var user = new User(UserId.New())
        {
            PersonalId = request.PersonalId,
            Username = request.FullName.ToLowerInvariant(),
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BC.EnhancedHashPassword(request.Password),
            AddressInfo = new FullAddress(request.Country,
                request.State ?? request.City,
                request.City,
                request.ZipCode,
                request.Address),
            CultureInfo = request.CultureInfo,
            TimeZone = request.TimeZoneInfo,
        };

        user.AddDomainEvent(new UserRegistered(user.Id));

        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        var loginCommand = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            TimeZoneInfo = request.TimeZoneInfo,
        };

        var res = await sender.Send(loginCommand, ct);
        return res is not null ? (res.Value.Token, res.Value.User)! : null;
    }
}