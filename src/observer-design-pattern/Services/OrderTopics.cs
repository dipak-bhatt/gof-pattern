namespace ObserverDesignPattern.Services;

/// <summary>
/// The topic names used by the demo. Keeping them in one place avoids a typo
/// silently subscribing an observer to a topic nobody publishes to.
/// </summary>
public static class OrderTopics
{
    public const string StatusChanged = "orders.status.changed";
}
