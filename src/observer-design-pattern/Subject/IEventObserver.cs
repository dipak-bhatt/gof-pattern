namespace ObserverDesignPattern.Subject;

/// <summary>
/// The Observer contract. Every subscriber implements this interface, and the
/// subject knows nothing else about it.
/// </summary>
/// <typeparam name="TEvent">The event type this observer handles.</typeparam>
public interface IEventObserver<in TEvent>
{
    /// <summary>A readable name used in the demo output and in failure logs.</summary>
    string Name { get; }

    /// <summary>
    /// Reacts to one event. Throwing means "not handled"; the bus reports the
    /// failure and continues with the next observer.
    /// </summary>
    Task OnEventAsync(TEvent @event, CancellationToken cancellationToken = default);
}
