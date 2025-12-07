using Signals.Core.Bus;
using Signals.Core.Events;
using Signals.Core.Modules;
using Signals.Runtime.Manifest;

namespace Signals.ApiBase.Fluent;

/// <summary>
/// Base class for creating fluent Signal modules.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides a convenient fluent API to subscribe to events via <see cref="IEventBus"/>.</item>
/// <item>Automatically tracks the module manifest for runtime metadata.</item>
/// <item>Derived classes implement <see cref="OnRegister"/> to register their events.</item>
/// <item>Supports subscribing handlers with or without <see cref="EventContext"/> parameter.</item>
/// <item>Intended for plugin/Signal modules where registration is declarative and minimal boilerplate is desired.</item>
/// </list>
/// </remarks>
public abstract class FluentSignalModuleBase : ISignalModule
{
    private IEventBus? _bus;

    private ModuleManifest Manifest { get; set; } = null!;

    protected void On<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        if (_bus == null) throw new InvalidOperationException("EventBus not initialized");
        _bus.Subscribe(handler);
    }

    protected void On<TEvent>(Func<TEvent, EventContext, Task> handler) where TEvent : IEvent
    {
        if (_bus == null) throw new InvalidOperationException("EventBus not initialized");
        _bus.Subscribe(handler);
    }

    public void RegisterSignals(IEventBus bus)
    {
        _bus = bus;
        Manifest = Manifest with { Dll = GetType().Assembly.GetName().Name + ".dll" };
        OnRegister();
    }

    public virtual void UnregisterSignals(IEventBus bus)
    {
        
    }
    
    protected virtual void OnRegister() { }
}