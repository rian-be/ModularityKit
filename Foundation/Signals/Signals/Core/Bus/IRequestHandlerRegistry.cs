using Signals.Core.Events;

namespace Signals.Core.Bus;

/// <summary>
/// Maintains a registry of request-response event handlers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Stores mappings from request events (<see cref="IEvent"/>) to their corresponding response handlers (<see cref="IRequestHandler{TRequest,TResponse}"/>).</item>
/// <item>Supports retrieval of registered handlers for a given request/response type pair.</item>
/// <item>Used by event bus implementations to route request events to the appropriate handler.</item>
/// </list>
/// </remarks>
public interface IRequestHandlerRegistry
{
    /// <summary>
    /// Retrieves a registered request handler for the specified request/response type pair.
    /// </summary>
    /// <typeparam name="TRequest">The request event type.</typeparam>
    /// <typeparam name="TResponse">The response event type.</typeparam>
    /// <returns>
    /// The registered <see cref="IRequestHandler{TRequest,TResponse}"/> if found; otherwise, <c>null</c>.
    /// </returns>
    IRequestHandler<TRequest, TResponse>? GetHandler<TRequest, TResponse>()
        where TRequest : IEvent
        where TResponse : IResponseEvent;

    /// <summary>
    /// Registers a request handler for a specific request/response type pair.
    /// </summary>
    /// <typeparam name="TRequest">The request event type.</typeparam>
    /// <typeparam name="TResponse">The response event type.</typeparam>
    /// <param name="handler">The handler instance to register.</param>
    void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IEvent
        where TResponse : IResponseEvent;
}
