using Signals.Core.Bus;

namespace Signals.Core.Events;

/// <summary>
/// Defines a handler for a specific type of <see cref="IEvent"/>.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Used to implement strongly-typed event handling logic for a given event type.</item>
/// <item>Handlers implementing this interface can be registered with an <see cref="IEventBus"/>.</item>
/// <item>Supports asynchronous processing of events via <see cref="Task"/>.</item>
/// </list>
/// </remarks>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="evt">The event instance to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Handle(TEvent evt);
}