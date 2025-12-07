using Signals.Core.Bus;

namespace Signals.Core.Events;

/// <summary>
/// Defines a handler for a request-response pattern using events.
/// </summary>
/// <typeparam name="TRequest">The type of the request event.</typeparam>
/// <typeparam name="TResponse">The type of the response event.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Processes a specific request event (<typeparamref name="TRequest"/>) and produces a corresponding response event (<typeparamref name="TResponse"/>).</item>
/// <item>Used in scenarios where a request requires an asynchronous response.</item>
/// <item>Integrates with event-driven pipelines and can be registered in an <see cref="IEventBus"/>.</item>
/// </list>
/// </remarks>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IEvent
    where TResponse : IResponseEvent
{
    Task<TResponse> Handle(TRequest request);
}