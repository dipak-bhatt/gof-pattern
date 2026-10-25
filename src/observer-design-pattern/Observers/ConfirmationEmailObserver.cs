using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Observers;

/// <summary>
/// Sends the customer a receipt. It cares about one thing only: an order
/// became <see cref="OrderStatus.Paid"/>.
/// </summary>
public sealed class ConfirmationEmailObserver : IEventObserver<OrderStatusChanged>
{
    public string Name => "confirmation-email";

    public Task OnEventAsync(OrderStatusChanged @event, CancellationToken cancellationToken = default)
    {
        if (@event.Status == OrderStatus.Paid)
            Console.WriteLine($"  confirmation-email: receipt for {@event.OrderId} sent to {@event.Customer}");

        return Task.CompletedTask;
    }
}
