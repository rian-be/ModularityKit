using Signals.Core.Events;

namespace Signals.Core.Bus;

/// <summary>
/// Represents an event bus for publishing and subscribing to events.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Allows Eventlets or other components to publish events implementing <see cref="IEvent"/>.</item>
/// <item>Allows subscription to specific event types with strongly-typed handlers.</item>
/// <item>Supports multiple subscribers per event type; all handlers will be invoked when an event is published.</item>
/// <item>Supports priorities, filters, one-time subscriptions, and batch publishing.</item>
/// </list>
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publishes a single event asynchronously to all registered handlers.
    /// </summary>
    /// <param name="evt">The event to publish.</param>
    Task Publish(IEvent evt);

    /// <summary>
    /// Publishes multiple events asynchronously.
    /// </summary>
    /// <param name="events">The events to publish.</param>
    Task PublishBatch(params IEvent[] events);

    /// <summary>
    /// Subscribes a handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The async handler function.</param>
    /// <param name="priority">Optional priority (higher executes first).</param>
    /// <param name="filter">Optional filter function; handler is called only if filter returns true.</param>
    void Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent;

    void Subscribe<TEvent>(Func<TEvent, EventContext, Task> handler) where TEvent : IEvent;

    Task<TResponse> Send<TRequest, TResponse>(TRequest request)
        where TRequest : IEvent
        where TResponse : IResponseEvent;

    /// <summary>
    /// Subscribes a one-time handler for a specific event type. Handler is removed after first execution.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The async handler function.</param>
    /// <param name="priority">Optional priority (higher executes first).</param>
    /// <param name="filter">Optional filter function; handler is called only if filter returns true.</param>
    void SubscribeOnce<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes a previously registered handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The async handler function to remove.</param>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
    
    void Unsubscribe<TEvent>(Func<TEvent, EventContext, Task> handler) where TEvent : IEvent;

    
    void Subscribe(Type eventType, Func<IEvent, Task> handler);
    void Unsubscribe(Type eventType, Func<IEvent, Task> handler);
}
