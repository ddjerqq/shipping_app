using System.Security.Cryptography;
using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;
using EntityFrameworkCore.DataProtection.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Commands;

public sealed record CreatePersonalPackageCommand : IRequest<Package>
{
    public User? Sender { get; set; }
    public bool CreateSender { get; set; }

    [LogMasked]
    public string? SenderUsername { get; set; }

    [LogMasked]
    public string? SenderId { get; set; }

    [LogMasked]
    public string? SenderMobileNumber { get; set; }

    [LogMasked]
    public string? SenderHomeNumber { get; set; }

    [LogMasked]
    public string? SenderEmail { get; set; }

    public User? Receiver { get; set; }
    public bool CreateReceiver { get; set; }

    [LogMasked]
    public string? ReceiverUsername { get; set; }

    [LogMasked]
    public string? ReceiverId { get; set; }

    [LogMasked]
    public string? ReceiverMobileNumber { get; set; }

    [LogMasked]
    public string? ReceiverHomeNumber { get; set; }

    [LogMasked]
    public string? ReceiverEmail { get; set; }

    public Money? PricePerKg { get; set; }
}

public sealed class CreatePersonalPackageValidator : AbstractValidator<CreatePersonalPackageCommand>
{
    public CreatePersonalPackageValidator(IAppDbContext dbContext)
    {
        RuleFor(command => command.Sender).NotEqual(command => command.Receiver).WithMessage("{PropertyName} must be unique").When(command => command.Sender != null);
        RuleFor(command => command.Receiver).NotEqual(command => command.Sender).WithMessage("{PropertyName} must be unique").When(command => command.Receiver != null);

        When(command => command.CreateReceiver && command.CreateSender, () =>
        {
            // Sender ID
            RuleFor(command => command.SenderId)
                .NotEqual(command => command.ReceiverId)
                .WithMessage("Id must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.SenderId) && !string.IsNullOrWhiteSpace(command.ReceiverId));

            // Sender Mobile Number
            RuleFor(command => command.SenderMobileNumber)
                .NotEqual(command => command.ReceiverMobileNumber)
                .WithMessage("Mobile number must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.SenderMobileNumber) && !string.IsNullOrWhiteSpace(command.ReceiverMobileNumber));

            // Sender Home Number
            RuleFor(command => command.SenderHomeNumber)
                .NotEqual(command => command.ReceiverHomeNumber)
                .WithMessage("Home number must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.SenderHomeNumber) && !string.IsNullOrWhiteSpace(command.ReceiverHomeNumber));

            // Sender Username & Email
            RuleFor(command => command.SenderUsername)
                .NotEqual(command => command.ReceiverUsername)
                .WithMessage("Username must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.SenderUsername) && !string.IsNullOrWhiteSpace(command.ReceiverUsername));

            RuleFor(command => command.SenderEmail)
                .NotEqual(command => command.ReceiverEmail)
                .WithMessage("Email must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.SenderEmail) && !string.IsNullOrWhiteSpace(command.ReceiverEmail));

            // Receiver ID
            RuleFor(command => command.ReceiverId)
                .NotEqual(command => command.SenderId)
                .WithMessage("Id must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.ReceiverId) && !string.IsNullOrWhiteSpace(command.SenderId));

            // Receiver Mobile Number
            RuleFor(command => command.ReceiverMobileNumber)
                .NotEqual(command => command.SenderMobileNumber)
                .WithMessage("Mobile number must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.ReceiverMobileNumber) && !string.IsNullOrWhiteSpace(command.SenderMobileNumber));

            // Receiver Home Number
            RuleFor(command => command.ReceiverHomeNumber)
                .NotEqual(command => command.SenderHomeNumber)
                .WithMessage("Home number must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.ReceiverHomeNumber) && !string.IsNullOrWhiteSpace(command.SenderHomeNumber));

            // Receiver Username & Email
            RuleFor(command => command.ReceiverUsername)
                .NotEqual(command => command.SenderUsername)
                .WithMessage("Username must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.ReceiverUsername) && !string.IsNullOrWhiteSpace(command.SenderUsername));

            RuleFor(command => command.ReceiverEmail)
                .NotEqual(command => command.SenderEmail)
                .WithMessage("Email must be unique")
                .When(command => !string.IsNullOrWhiteSpace(command.ReceiverEmail) && !string.IsNullOrWhiteSpace(command.SenderEmail));
        });

        RuleFor(x => x.SenderUsername).NotEmpty().MinimumLength(5).MaximumLength(32)
            .When(command => command.CreateSender);
        RuleFor(x => x.SenderEmail).NotEmpty().EmailAddress()
            .When(command => command.CreateSender);

        RuleFor(command => command.SenderId)
            .Matches(@"^\d{11}$|^\d{3}-\d{4}-\d{3}$")
            .WithMessage("Must be 11-digit Georgian ID or 3-4-3 digits American SSN")
            .When(command => !string.IsNullOrWhiteSpace(command.SenderId) && command.CreateSender);

        RuleFor(command => command.SenderMobileNumber)
            .Must(x => x!.TryParsePhoneNumber(out _))
            .WithMessage("{PropertyName} is invalid")
            .When(command => !string.IsNullOrWhiteSpace(command.SenderMobileNumber) && command.CreateSender);

        RuleFor(command => command.SenderHomeNumber)
            .Must(x => x!.TryParsePhoneNumber(out _))
            .WithMessage("{PropertyName} is invalid")
            .When(command => !string.IsNullOrWhiteSpace(command.SenderHomeNumber) && command.CreateSender);

        RuleFor(x => x.ReceiverUsername).NotEmpty().MinimumLength(5).MaximumLength(32)
            .When(command => command.CreateReceiver);
        RuleFor(x => x.ReceiverEmail).NotEmpty().EmailAddress()
            .When(command => command.CreateReceiver);

        RuleFor(command => command.ReceiverId)
            .Matches(@"^\d{11}$|^\d{3}-\d{4}-\d{3}$")
            .WithMessage("Must be 11-digit Georgian ID or 3-4-3 digits American SSN")
            .When(command => !string.IsNullOrWhiteSpace(command.ReceiverId) && command.CreateReceiver);

        RuleFor(command => command.ReceiverMobileNumber)
            .Must(x => x!.TryParsePhoneNumber(out _))
            .WithMessage("{PropertyName} is invalid")
            .When(command => !string.IsNullOrWhiteSpace(command.ReceiverMobileNumber) && command.CreateReceiver);

        RuleFor(command => command.ReceiverHomeNumber)
            .Must(x => x!.TryParsePhoneNumber(out _))
            .WithMessage("{PropertyName} is invalid")
            .When(command => !string.IsNullOrWhiteSpace(command.ReceiverHomeNumber) && command.CreateReceiver);

        // Sender
        RuleFor(x => x.SenderEmail)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.Email), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateSender && !string.IsNullOrWhiteSpace(command.SenderEmail));

        RuleFor(x => x.SenderMobileNumber)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateSender && !string.IsNullOrWhiteSpace(command.SenderMobileNumber));

        RuleFor(x => x.SenderHomeNumber)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.HomeNumber), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateSender && !string.IsNullOrWhiteSpace(command.SenderHomeNumber));

        RuleFor(x => x.SenderId)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PersonalId), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateSender && !string.IsNullOrWhiteSpace(command.SenderId));

        // Receiver
        RuleFor(x => x.ReceiverEmail)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.Email), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateReceiver && !string.IsNullOrWhiteSpace(command.ReceiverEmail));

        RuleFor(x => x.ReceiverMobileNumber)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateReceiver && !string.IsNullOrWhiteSpace(command.ReceiverMobileNumber));

        RuleFor(x => x.ReceiverHomeNumber)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.HomeNumber), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateReceiver && !string.IsNullOrWhiteSpace(command.ReceiverHomeNumber));

        RuleFor(x => x.ReceiverId)
            .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PersonalId), pd!).AnyAsync(ct))
            .WithMessage("User with {PropertyName} already exists")
            .When(command => command.CreateReceiver && !string.IsNullOrWhiteSpace(command.ReceiverId));
    }
}

internal sealed class CreatePersonalPackageCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreatePersonalPackageCommand, Package>
{
    public async Task<Package> Handle(CreatePersonalPackageCommand request, CancellationToken ct = default)
    {
        if (request.CreateSender)
        {
            var sanitizedPhone = request.SenderMobileNumber?.TryParsePhoneNumber(out var x) is true ? x : null;
            var sanitizedHome = request.SenderHomeNumber?.TryParsePhoneNumber(out var y) is true ? y : null;

            var sender = new User(UserId.New())
            {
                PersonalId = request.SenderId!,
                Username = request.SenderUsername?.ToLowerInvariant()!,
                Email = request.SenderEmail?.ToLowerInvariant()!,
                PhoneNumber = sanitizedPhone,
                HomeNumber = sanitizedHome,
            };

            var senderPassword = RandomNumberGenerator.GetHexString(12, true);

            sender.SetPassword(senderPassword, true);
            sender.ConfirmEmail();
            sender.AddDomainEvent(new UserAddedByAdmin(sender.Id, senderPassword));

            await dbContext.Users.AddAsync(sender, ct);
            await dbContext.SaveChangesAsync(ct);

            request.Sender = sender;
        }

        if (request.CreateReceiver)
        {
            var sanitizedPhone = request.ReceiverMobileNumber?.TryParsePhoneNumber(out var x) is true ? x : null;
            var sanitizedHome = request.ReceiverHomeNumber?.TryParsePhoneNumber(out var y) is true ? y : null;

            var receiver = new User(UserId.New())
            {
                PersonalId = request.ReceiverId!,
                Username = request.ReceiverUsername?.ToLowerInvariant()!,
                Email = request.ReceiverEmail?.ToLowerInvariant()!,
                PhoneNumber = sanitizedPhone,
                HomeNumber = sanitizedHome,
            };

            var receiverPassword = RandomNumberGenerator.GetHexString(12, true);

            receiver.SetPassword(receiverPassword, true);
            receiver.ConfirmEmail();
            receiver.AddDomainEvent(new UserAddedByAdmin(receiver.Id, receiverPassword));

            await dbContext.Users.AddAsync(receiver, ct);
            await dbContext.SaveChangesAsync(ct);

            request.Receiver = receiver;
        }

        var package = Package.CreatePersonal(
            Category.OtherConsumerProducts,
            request.Sender!,
            request.Receiver!,
            request.PricePerKg);

        dbContext.Packages.Add(package);
        await dbContext.SaveChangesAsync(ct);
        return package;
    }
}