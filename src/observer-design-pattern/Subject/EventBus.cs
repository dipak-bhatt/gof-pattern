namespace ObserverDesignPattern.Subject;

/// <summary>
/// An in-process implementation of the Observer pattern.
/// </summary>
/// <remarks>
/// The bus keeps a list of observers per topic. A publisher hands it an event;
/// the bus looks up the topic and calls each observer in turn. The publisher
/// never holds a reference to an observer, so observers can be attached or
/// detached while the application is running.
///
/// Each observer is isolated: one failure is reported and the remaining
/// observers still receive the event. In a message broker this is the same
/// reason each consumer has its own delivery and retry state.
/// </remarks>
public sealed class EventBus(Action<string>? log = null) : IEventBus
{
    private readonly Dictionary<string, List<IRegistration>> _subscribers = [];
    private readonly object _lock = new();
    private readonly Action<string> _log = log ?? Console.WriteLine;

    public IDisposable Subscribe<TEvent>(string topic, IEventObserver<TEvent> observer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(observer);

        var registration = new Registration<TEvent>(observer);

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(topic, out var registrations))
            {
                registrations = [];
                _subscribers[topic] = registrations;
            }

            registrations.Add(registration);
        }

        return new Subscription(() => Remove(topic, registration));
    }

    public async Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        IRegistration[] recipients;

        // Copy the list before awaiting: an observer may attach or detach
        // another observer while the event is being delivered.
        lock (_lock)
        {
            recipients = _subscribers.TryGetValue(topic, out var registrations)
                ? [.. registrations]
                : [];
        }

        foreach (var recipient in recipients)
        {
            try
            {
                await recipient.HandleAsync(@event!, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _log($"  bus: observer '{recipient.ObserverName}' failed: {exception.Message}");
                _log("       the other observers continue; this one is simply skipped");
            }
        }
    }

    private void Remove(string topic, IRegistration registration)
    {
        lock (_lock)
        {
            if (_subscribers.TryGetValue(topic, out var registrations))
                registrations.Remove(registration);
        }
    }

    /// <summary>The non-generic view the bus keeps in its dictionary.</summary>
    private interface IRegistration
    {
        string ObserverName { get; }
        Task HandleAsync(object @event, CancellationToken cancellationToken);
    }

    /// <summary>Binds an observer to the event type it handles.</summary>
    private sealed class Registration<TEvent>(IEventObserver<TEvent> observer) : IRegistration
    {
        public string ObserverName => observer.Name;

        public Task HandleAsync(object @event, CancellationToken cancellationToken) =>
            observer.OnEventAsync((TEvent)@event, cancellationToken);
    }

    /// <summary>The handle returned by <see cref="Subscribe{TEvent}"/>; disposal detaches.</summary>
    private sealed class Subscription(Action detach) : IDisposable
    {
        private Action? _detach = detach;

        public void Dispose() => Interlocked.Exchange(ref _detach, null)?.Invoke();
    }
}
