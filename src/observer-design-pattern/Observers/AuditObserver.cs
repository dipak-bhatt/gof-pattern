using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Observers;

/// <summary>
/// Records every status change. This is the observer that stays useful for
/// events no business observer handles yet.
/// </summary>
public sealed class AuditObserver : IEventObserver<OrderStatusChanged>
{
    public string Name => "audit";

    public Task OnEventAsync(OrderStatusChanged @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"  audit: {@event.OrderId} -> {@event.Status} (total {Currency.Format(@event.Total)})");

        return Task.CompletedTask;
    }
}
