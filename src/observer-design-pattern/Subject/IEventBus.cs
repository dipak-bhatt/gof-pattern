namespace ObserverDesignPattern.Subject;

/// <summary>
/// The Subject contract: attach an observer, detach it (by disposing the
/// subscription), and notify every observer attached to a topic.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Attaches <paramref name="observer"/> to <paramref name="topic"/>.
    /// Dispose the returned handle to detach it.
    /// </summary>
    IDisposable Subscribe<TEvent>(string topic, IEventObserver<TEvent> observer);

    /// <summary>
    /// Notifies every observer attached to <paramref name="topic"/>.
    /// The publisher does not know which observers, or how many, are attached.
    /// </summary>
    Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        CancellationToken cancellationToken = default);
}
