using Application.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record TopUpBalanceCommand : IRequest<string>
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

    public Money Amount { get; set; } = new("USD", 0);
}

internal sealed class TopUpBalanceCommandHandler(ICurrentUserAccessor currentUser, IPaymentService paymentService) : IRequestHandler<TopUpBalanceCommand, string>
{
    public async Task<string> Handle(TopUpBalanceCommand request, CancellationToken ct)
    {
        if (request.PaymentMethod is not PaymentMethod.Card)
            throw new NotImplementedException("Only card payment is supported for now.");

        var user = await currentUser.GetCurrentUserAsync(ct);
        var paymentUrl = await paymentService.CreatePaymentUrl(user, request.Amount, ct);
        return paymentUrl;
    }
}