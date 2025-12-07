using System.Collections.Concurrent;
using Signals.Core.Bus;
using Signals.Core.Events;
using Signals.Core.Handlers;

namespace Signals.Core.Subscriptions;

/// <summary>
/// Manages subscriptions of event and request handlers within the event bus.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Registers regular and one-time event handlers for <see cref="IEvent"/> types.</item>
/// <item>Supports handler priorities and optional event filtering.</item>
/// <item>Allows removal of previously registered handlers.</item>
/// <item>Integrates request/response handlers via <see cref="IRequestHandlerRegistry"/>.</item>
/// <item>Intended for internal use by <see cref="IEventBus"/> implementations.</item>
/// <item>Implementations must be thread-safe.</item>
/// </list>
/// </remarks>
public sealed class SubscriptionManager(ConcurrentDictionary<Type, HandlerCollection> handlers) : ISubscriptionManager
{
    /// <inheritdoc />
    public void Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent
        => AddHandler(handler, once: false, priority, filter);

    /// <inheritdoc />
    public void SubscribeOnce<TEvent>(Func<TEvent, Task> handler, int priority = 0, Func<TEvent, bool>? filter = null) where TEvent : IEvent
        => AddHandler(handler, once: true, priority, filter);

    /// <inheritdoc />
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        if (handlers.TryGetValue(typeof(TEvent), out var collection))
            collection.Unsubscribe(e => handler((TEvent)e));
    }

    private void AddHandler<TEvent>(Func<TEvent, Task> handler, bool once, int priority, Func<TEvent, bool>? filter) where TEvent : IEvent
    {
        var wrapper = new HandlerWrapper(
            Handler: e => handler((TEvent)e),
            Priority: priority,
            Filter: filter is not null ? new Func<IEvent, bool>(e => filter((TEvent)e)) : null,
            Once: once
        );

        handlers.GetOrAdd(typeof(TEvent), _ => new HandlerCollection()).Add(wrapper);
    }
    
    
    

    public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IEvent
        where TResponse : IResponseEvent
    {
        var collection = handlers.GetOrAdd(typeof(TRequest), _ => new HandlerCollection());
        //collection.Clear();
        collection.Add(new HandlerWrapper(
            Handler: e => handler.Handle((TRequest)e),
            Priority: 0,
            Filter: null,
            Once: false
        ));
    }
    
    public IRequestHandler<TRequest, TResponse>? GetHandler<TRequest, TResponse>()
        where TRequest : IEvent
        where TResponse : IResponseEvent
    {
        if (handlers.TryGetValue(typeof(TRequest), out var collection))
        {
            var wrapper = collection.GetSnapshot(null!).FirstOrDefault();
            if (wrapper != null)
            {
                return new FuncHandler<TRequest, TResponse>(wrapper);
            }
        }
        return null;
    }
    
    private sealed class FuncHandler<TRequest, TResponse>(HandlerWrapper wrapper) : IRequestHandler<TRequest, TResponse>
        where TRequest : IEvent
        where TResponse : IResponseEvent
    {
        public Task<TResponse> Handle(TRequest request)
        {
            return (Task<TResponse>)wrapper.Handler(request);
        }
    }
    
}
