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
    /// <summary>
    /// Calculates simple password entropy based on length and character sets used.
    /// </summary>
    /// <param name="password">The password string to analyze.</param>
    /// <returns>An integer representing the calculated entropy score.</returns>
    private static int CalculatePasswordEntropy(string password)
    {
        // Return 0 if the password is null or empty
        if (string.IsNullOrEmpty(password))
        {
            return 0;
        }

        int poolSize = 0;
        int passwordLength = password.Length;

        // Check for lowercase letters (a-z)
        if (password.Any(char.IsLower))
        {
            poolSize += 26;
        }

        // Check for uppercase letters (A-Z)
        if (password.Any(char.IsUpper))
        {
            poolSize += 26;
        }

        // Check for digits (0-9)
        if (password.Any(char.IsDigit))
        {
            poolSize += 10;
        }

        // Check for symbols (non-alphanumeric characters)
        // We'll use a common set size of 32 for symbols
        if (password.Any(c => !char.IsLetterOrDigit(c)))
        {
            poolSize += 32;
        }

        // If no character sets were found (highly unlikely for non-empty), return 0
        if (poolSize == 0)
        {
            return 0;
        }

        // Calculate entropy using the formula: E = L * log2(R)
        // E = Entropy
        // L = Password Length
        // R = Pool Size (sum of character set sizes)
        double entropy = passwordLength * Math.Log(poolSize, 2);

        // Return the entropy as an integer
        return (int)entropy;
    }
    public RegisterCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(5).MaximumLength(32);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256).Must(password => CalculatePasswordEntropy(password) > 50)
            .WithMessage("Please choose a stronger password, try including symbols and uppercase characters");
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