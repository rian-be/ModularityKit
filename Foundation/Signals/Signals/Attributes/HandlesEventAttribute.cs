using Signals.Core.Events;

namespace Signals.Attributes;

/// <summary>
/// Marks a class or method as a handler for a specific event type.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used for automatic discovery and registration of event handlers via reflection.</item>
/// <item>Can be applied to both classes and individual methods.</item>
/// <item>Supports multiple attributes on a single target to handle multiple event types.</item>
/// <item>Validates at runtime that the provided type implements <see cref="IEvent"/>.</item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HandlesEventAttribute : Attribute
{
    public Type EventType { get; }
    
    public HandlesEventAttribute(Type eventType)
    {
        if (!typeof(IEvent).IsAssignableFrom(eventType))
            throw new ArgumentException("EventType must implement IEvent");
        EventType = eventType;
    }
}