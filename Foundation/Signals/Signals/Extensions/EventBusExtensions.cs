using Signals.Core.Bus;
using Signals.Core.Events;
using Signals.Runtime.Loader;

namespace Signals.Extensions;

/// <summary>
/// Provides dynamic publishing helpers for <see cref="IEventBus"/> using runtime-loaded Signals.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Allows publishing events using string-based Signal and event identifiers.</item>
/// <item>Resolves event types at runtime via <see cref="SignalsLoader"/>.</item>
/// <item>Creates event instances using reflection and constructor arguments.</item>
/// <item>Invokes the standard <see cref="IEventBus.Publish(IEvent)"/> pipeline.</item>
/// <item>Intended for dynamic/plugin-based Signal execution scenarios.</item>
/// </list>
/// </remarks>
public static class EventBusExtensions
{
    public static async Task Publish(
        this IEventBus bus,
        SignalsLoader loader,
        string signalsId,
        string eventName,
        params object[] ctorArgs)
    {
        var type = loader.GetPluginEventType(signalsId, eventName);
        if (type == null)
            throw new InvalidOperationException($"Event '{eventName}' not found in Signal '{signalsId}'.");

        var instance = (IEvent)Activator.CreateInstance(type, ctorArgs)!;
        await bus.Publish(instance);
    }
}