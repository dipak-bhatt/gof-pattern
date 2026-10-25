namespace ObserverDesignPattern.Models;

/// <summary>
/// The lifecycle states used by the demo. Observers decide which states
/// matter to them; the publisher does not make that choice.
/// </summary>
public enum OrderStatus
{
    Placed,
    Paid,
    Shipped,
    Cancelled
}
