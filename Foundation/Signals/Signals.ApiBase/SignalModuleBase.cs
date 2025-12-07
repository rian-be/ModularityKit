using Signals.ApiBase.Registrar;
using Signals.Core.Bus;
using Signals.Core.Modules;
using Signals.Core.Subscriptions;

namespace Signals.ApiBase;

/// <summary>
/// Base class for Signal modules. Automatically scans assembly for event and request/response handlers
/// and registers them using <see cref="IEventBus"/> and <see cref="IRequestHandlerRegistry"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Scans the derived module's assembly for all handler types.</item>
/// <item>Registers handlers via a set of <see cref="IHandlerRegistrar"/> implementations.</item>
/// <item>Supports future extension for Streams, Sagas, Cron jobs, etc.</item>
/// <item>Provides a clear extension point for <see cref="UnregisterSignals"/> (currently not implemented).</item>
/// </list>
/// </remarks>
public abstract class SignalModuleBase : ISignalModule
{
    private static readonly IHandlerRegistrar[] Registrars =
    [
        new RequestHandlerRegistrar(),
        new EventHandlerRegistrar(),
        // new StreamHandlerRegistrar(),
        // new SagaHandlerRegistrar(),
        // new CronHandlerRegistrar()
    ];

    public void RegisterSignals(IEventBus bus)
    {
        if (bus == null) throw new ArgumentNullException(nameof(bus));

        var subscriptionManager = ExtractSubscriptionManager(bus);
        var registry = subscriptionManager as IRequestHandlerRegistry;

        var handlerTypes = new HandlerTypeScanner(GetType().Assembly).GetHandlers();

        foreach (var handlerType in handlerTypes)
        {
            foreach (var registrar in Registrars)
            {
                if (registrar.TryRegister(handlerType, bus, registry))
                    break;
            }
        }
    }

    public void UnregisterSignals(IEventBus bus) => throw new NotImplementedException();

    private static ISubscriptionManager ExtractSubscriptionManager(IEventBus bus)
    {
        if (bus is not EventBus concreteBus)
            throw new InvalidOperationException("Expected EventBus with SubscriptionManager property");

        return concreteBus.SubscriptionManager
               ?? throw new InvalidOperationException("SubscriptionManager cannot be null");
    }
}
