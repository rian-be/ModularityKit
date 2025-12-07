using Signals.Core.Bus;

namespace Signals.ApiBase.Registrar;

/// <summary>
/// Defines a handler registrar capable of scanning a type and registering it
/// as either an event handler or a request/response handler into the corresponding bus/registry.
/// </summary>
public interface IHandlerRegistrar
{
    /// <summary>
    /// Attempts to register the given <paramref name="handlerType"/> into the <paramref name="bus"/>
    /// or <paramref name="registry"/>. Returns true if registration was successful.
    /// </summary>
    /// <param name="handlerType">The type implementing a handler.</param>
    /// <param name="bus">Event bus for event handlers.</param>
    /// <param name="registry">Request handler registry for request/response handlers.</param>
    /// <returns>True if the handler was successfully registered; otherwise, false.</returns>
    bool TryRegister(Type handlerType, IEventBus bus, IRequestHandlerRegistry? registry);
}