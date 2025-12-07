using Signals.Core.Events;

namespace Signals.Attributes;

/// <summary>
/// Marks a class as a handler for a specific request/response event pair.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used for automatic discovery and registration of <see cref="IRequestHandler{TRequest,TResponse}"/> implementations.</item>
/// <item>Associates a concrete request event type with its corresponding response event type.</item>
/// <item>Intended for use with reflection-based handler scanning.</item>
/// <item>Must be applied to non-abstract handler classes.</item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HandlesRequestAttribute(Type requestType, Type responseType) : Attribute
{
    public Type RequestType { get; } = requestType;
    public Type ResponseType { get; } = responseType;
}