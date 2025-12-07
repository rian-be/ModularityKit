namespace Signals.Core.Events;

/// <summary>
/// Represents an event that is intended as a response to another event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IEvent"/> and can be published or consumed like a normal event.</item>
/// <item>Typically used in request-response patterns where a handler produces a response event.</item>
/// <item>Can be handled by regular event handlers or by specialized responders.</item>
/// </list>
/// </remarks>
public interface IResponseEvent : IEvent { }