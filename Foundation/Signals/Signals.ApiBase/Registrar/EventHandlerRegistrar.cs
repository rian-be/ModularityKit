using Signals.Attributes;
using Signals.Core.Bus;
using Signals.Core.Events;

namespace Signals.ApiBase.Registrar;

/// <summary>
/// Registers standard <see cref="IEventHandler{TEvent}"/> implementations
/// decorated with <see cref="HandlesEventAttribute"/> into the event bus.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Scans the provided type for <see cref="HandlesEventAttribute"/>.</item>
/// <item>Resolves the corresponding <see cref="IEventHandler{TEvent}"/> interface.</item>
/// <item>Creates a handler instance via <see cref="Activator"/>.</item>
/// <item>Registers a wrapper delegate in <see cref="IEventBus"/> for runtime invocation.</item>
/// </list>
/// </remarks>
public sealed class EventHandlerRegistrar: IHandlerRegistrar
{
    /// <inheritdoc />
    public bool TryRegister(Type handlerType, IEventBus bus, IRequestHandlerRegistry? registry)
    {
        var eventAttr = handlerType.GetCustomAttributes(typeof(HandlesEventAttribute), inherit: false)
            .Cast<HandlesEventAttribute>()
            .FirstOrDefault();
        if (eventAttr == null) return false;

        var eventType = eventAttr.EventType;

        var eventHandlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
                i.GetGenericArguments()[0] == eventType);

        if (eventHandlerInterface == null) return false;

        var handlerInstance = Activator.CreateInstance(handlerType)
                              ?? throw new InvalidOperationException($"Cannot create instance of {handlerType.FullName}");

        Task Wrapper(IEvent evt)
        {
            var methodInfo = eventHandlerInterface.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler {handlerType.FullName} nie ma metody Handle");

            return (Task)methodInfo.Invoke(handlerInstance, [evt])!;
        }

        bus.Subscribe(eventType, Wrapper);

        return true;
    }
}