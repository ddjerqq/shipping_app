using Application.Services;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Stripe.Checkout;

namespace Application.Cqrs.Users.Events;

public sealed class UserBalanceTopUpEventHandler(IAppDbContext dbContext, IUserNotifier notifier) : INotificationHandler<UserBalanceTopUp>
{
    public async Task Handle(UserBalanceTopUp notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct);

        object session;

        if (notification.PaymentMethod is PaymentMethod.Card)
        {
            var sessionService = new SessionService();
            session = await sessionService.GetAsync(notification.PaymentSessionId as string, cancellationToken: ct);
        }
        else
        {
            throw new NotImplementedException("Only card payment method is supported for now.");
        }

        await notifier.NotifyTopUpSuccess(user!, notification.Amount, notification.PaymentMethod, session, ct);
    }
}