using System.Collections.Concurrent;
using Signals.Core.Bus;
using Signals.Core.Events;

namespace Signals.Core.Subscriptions;

/// <summary>
/// Provides methods to manage subscriptions of event handlers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports subscribing regular and one-time handlers for <see cref="IEvent"/> types.</item>
/// <item>Allows optional filtering and priority ordering of handlers.</item>
/// <item>Supports unsubscribing previously registered handlers.</item>
/// <item>Designed to be used internally by <see cref="IEventBus"/> implementations.</item>
/// <item>Thread-safe via <see cref="ConcurrentDictionary{TKey,TValue}"/>.</item>
/// </list>
/// </remarks>
public interface ISubscriptionManager : IRequestHandlerRegistry
{
    /// <summary>
    /// Subscribes a handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">Asynchronous handler function.</param>
    /// <param name="priority">Handler priority; higher values invoked first.</param>
    /// <param name="filter">Optional predicate to filter events.</param>
    void Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent;

    /// <summary>
    /// Subscribes a one-time handler for a specific event type.
    /// The handler is removed after the first invocation.
    /// </summary>
    /// <typeparam name="TEvent">Type of event. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">Asynchronous handler function.</param>
    /// <param name="priority">Handler priority; higher values invoked first.</param>
    /// <param name="filter">Optional predicate to filter events.</param>
    void SubscribeOnce<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes a previously registered handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">Handler function to remove.</param>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
}