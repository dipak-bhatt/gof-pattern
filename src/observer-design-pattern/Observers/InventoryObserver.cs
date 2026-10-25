using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Observers;

/// <summary>
/// Reserves stock after payment. The publisher does not call an inventory
/// service directly, so inventory can be changed without touching the order
/// workflow.
/// </summary>
public sealed class InventoryObserver : IEventObserver<OrderStatusChanged>
{
    public string Name => "inventory";

    public Task OnEventAsync(OrderStatusChanged @event, CancellationToken cancellationToken = default)
    {
        if (@event.Status == OrderStatus.Paid)
            Console.WriteLine($"  inventory: stock reserved for order {@event.OrderId}");

        return Task.CompletedTask;
    }
}
