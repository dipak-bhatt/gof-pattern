using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Observers;

/// <summary>
/// A late subscriber added while the demo is running. No publisher and no
/// existing observer changes to make room for it.
/// </summary>
public sealed class AnalyticsObserver : IEventObserver<OrderStatusChanged>
{
    public string Name => "analytics";

    public Task OnEventAsync(OrderStatusChanged @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  analytics: counted one {@event.Status} event");
        return Task.CompletedTask;
    }
}
