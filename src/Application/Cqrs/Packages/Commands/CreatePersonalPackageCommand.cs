using System.Security.Cryptography;
using Application.Services;
using Domain.Aggregates;
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
    public string? SenderUsername { get; set; }
    public string? SenderId { get; set; }
    public string? SenderPhone { get; set; }
    public string? SenderEmail { get; set; }

    public User? Receiver { get; set; }
    public bool CreateReceiver { get; set; }
    public string? ReceiverUsername { get; set; }
    public string? ReceiverId { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverEmail { get; set; }

    public Money? PricePerKg { get; set; }
}

public sealed class CreatePersonalPackageValidator : AbstractValidator<CreatePersonalPackageCommand>
{
    public CreatePersonalPackageValidator(IAppDbContext dbContext)
    {
        RuleFor(command => command.Sender).NotEqual(command => command.Receiver).WithMessage("{PropertyName} must be unique").When(command => command.Sender != null);
        RuleFor(command => command.Receiver).NotEqual(command => command.Sender).WithMessage("{PropertyName} must be unique").When(command => command.Receiver != null);

        When(command => command.CreateSender, () =>
        {
            RuleFor(x => x.SenderUsername).NotEmpty().MinimumLength(5).MaximumLength(32);
            RuleFor(x => x.SenderEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.SenderPhone).NotEmpty().Matches(@"\d{3}\d{9}").WithMessage("Please enter international standard phone number (eg. 995599123123)");
            RuleFor(x => x.SenderId).NotEmpty().Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");

            RuleSet("async", () =>
            {
                RuleFor(x => x.SenderEmail)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.Email), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");

                RuleFor(x => x.SenderPhone)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");

                RuleFor(x => x.SenderId)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PersonalId), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");
            });
        });

        When(command => command.CreateReceiver, () =>
        {
            RuleFor(x => x.ReceiverUsername).NotEmpty().MinimumLength(5).MaximumLength(32);
            RuleFor(x => x.ReceiverEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.ReceiverPhone).NotEmpty().Matches(@"\d{3}\d{9}").WithMessage("Please enter international standard phone number (eg. 995599123123)");
            RuleFor(x => x.ReceiverId).NotEmpty().Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");

            RuleSet("async", () =>
            {
                RuleFor(x => x.ReceiverEmail)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.Email), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");

                RuleFor(x => x.ReceiverPhone)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PhoneNumber), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");

                RuleFor(x => x.ReceiverId)
                    .MustAsync(async (pd, ct) => !await dbContext.Users.WherePdEquals(nameof(User.PersonalId), pd!).AnyAsync(ct))
                    .WithMessage("User with {PropertyName} already exists");
            });
        });
    }
}

internal sealed class CreatePersonalPackageCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreatePersonalPackageCommand, Package>
{
    public async Task<Package> Handle(CreatePersonalPackageCommand request, CancellationToken ct = default)
    {
        if (request.CreateSender)
        {
            var sender = new User(UserId.New())
            {
                PersonalId = request.SenderId!,
                Username = request.SenderUsername!,
                Email = request.SenderEmail!,
                PhoneNumber = request.SenderPhone!,
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
            var receiver = new User(UserId.New())
            {
                PersonalId = request.ReceiverId!,
                Username = request.ReceiverUsername!,
                Email = request.ReceiverEmail!,
                PhoneNumber = request.ReceiverPhone!,
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