using ObserverDesignPattern.Models;
using ObserverDesignPattern.Subject;

namespace ObserverDesignPattern.Services;

/// <summary>
/// The publisher (the "concrete subject" in GoF terms).
/// </summary>
/// <remarks>
/// It depends on <see cref="IEventBus"/> only. It has no field for an e-mail
/// sender, an inventory service or an audit log: those are observers that
/// attach elsewhere. Adding a new reaction never changes this class.
/// </remarks>
public sealed class OrderService(IEventBus bus)
{
    public Task PlaceAsync(
        string orderId,
        string customer,
        decimal total,
        CancellationToken cancellationToken = default) =>
        PublishAsync(orderId, customer, total, OrderStatus.Placed, cancellationToken: cancellationToken);

    public Task PayAsync(
        string orderId,
        string customer,
        decimal total,
        CancellationToken cancellationToken = default) =>
        PublishAsync(orderId, customer, total, OrderStatus.Paid, cancellationToken: cancellationToken);

    private Task PublishAsync(
        string orderId,
        string customer,
        decimal total,
        OrderStatus status,
        string? note = null,
        CancellationToken cancellationToken = default) =>
        bus.PublishAsync(
            OrderTopics.StatusChanged,
            new OrderStatusChanged(orderId, customer, status, total, note),
            cancellationToken);
}
