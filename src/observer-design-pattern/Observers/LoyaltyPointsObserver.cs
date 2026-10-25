using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Observers;

/// <summary>
/// Awards loyalty points and can be told to fail for one order. The optional
/// failure shows that a broken observer does not stop the others.
/// </summary>
public sealed class LoyaltyPointsObserver(string? failOnOrderId = null)
    : IEventObserver<OrderStatusChanged>
{
    public string Name => "loyalty-points";

    public Task OnEventAsync(OrderStatusChanged @event, CancellationToken cancellationToken = default)
    {
        if (@event.Status != OrderStatus.Paid)
            return Task.CompletedTask;

        if (@event.OrderId == failOnOrderId)
            throw new InvalidOperationException("the loyalty service is unavailable");

        var points = (int)(@event.Total / 100);
        Console.WriteLine($"  loyalty-points: {@event.Customer} earned {points} points");
        return Task.CompletedTask;
    }
}
