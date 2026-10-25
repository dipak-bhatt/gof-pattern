namespace ObserverDesignPattern.Models;

/// <summary>
/// The event every observer receives.
/// </summary>
/// <remarks>
/// A record is immutable, so one observer cannot change the event that the
/// observers after it receive. The publisher creates it once and the bus
/// hands the same instance to each subscriber.
/// </remarks>
public sealed record OrderStatusChanged(
    string OrderId,
    string Customer,
    OrderStatus Status,
    decimal Total,
    string? Note = null);
