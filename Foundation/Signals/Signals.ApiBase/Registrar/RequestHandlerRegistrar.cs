using Signals.Attributes;
using Signals.Core.Bus;
using Signals.Core.Events;

namespace Signals.ApiBase.Registrar;

/// <summary>
/// Registers <see cref="IRequestHandler{TRequest,TResponse}"/> implementations
/// decorated with <see cref="HandlesRequestAttribute"/> into the request handler registry.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Scans the provided type for <see cref="HandlesRequestAttribute"/>.</item>
/// <item>Creates an instance via <see cref="Activator"/>.</item>
/// <item>Registers it in <see cref="IRequestHandlerRegistry"/> using <see cref="IRequestHandlerRegistry.RegisterHandler"/>.</item>
/// </list>
/// </remarks>
public sealed class RequestHandlerRegistrar : IHandlerRegistrar
{
    /// <inheritdoc />
    public bool TryRegister(Type handlerType, IEventBus bus, IRequestHandlerRegistry? registry)
    {
        if (registry == null) return false;

        var requestAttr = handlerType.GetCustomAttributes(typeof(HandlesRequestAttribute), inherit: false)
            .Cast<HandlesRequestAttribute>()
            .FirstOrDefault();
        if (requestAttr == null) return false;

        var handlerInstance = Activator.CreateInstance(handlerType)
                              ?? throw new InvalidOperationException($"Cannot create instance of {handlerType.FullName}");

        registry.RegisterHandler((dynamic)handlerInstance);

        return true;
    }
}
