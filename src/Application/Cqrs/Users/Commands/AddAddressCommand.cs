using Application.Services;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record AddAddressCommand : IRequest
{
    public string Country { get; set; } = null!;
    public string State { get; set; } = null!;
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Address { get; set; } = null!;
}

public sealed class AddAddressValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressValidator()
    {
        RuleFor(x => x.Country).NotEmpty().Matches("(GEO|USA)").WithMessage("Country must be either GEO or USA");
        RuleFor(x => x.State).NotEmpty().Matches(@"[a-zA-Z\s]+").WithMessage("State must contain only letters and spaces");
        RuleFor(x => x.City).NotEmpty().Matches(@"[a-zA-Z\s]+").WithMessage("City must contain only letters and spaces");
        RuleFor(x => x.ZipCode).NotEmpty().Matches(@"\d{3,5}").WithMessage("Zip code must be 3-5 digits");
        RuleFor(x => x.Address).NotEmpty().Matches(@"[a-zA-Z0-9\(\)\s]+").WithMessage("Address must contain only letters, digits, spaces, and parentheses");
    }
}

public sealed class AddAddressCommandHandler(ICurrentUserAccessor currentUser, IAppDbContext dbContext) : IRequestHandler<AddAddressCommand>
{
    public async Task Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(cancellationToken);
        user.AddressInfo = new FullAddress(request.Country, request.State, request.City, request.ZipCode, request.Address);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}