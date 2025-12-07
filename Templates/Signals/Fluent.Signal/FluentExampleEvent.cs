using Signals.Core.Events;

namespace Fluent.Signal;

/// <summary>
/// A sample implementation of <see cref="IEvent"/> used to demonstrate event publishing and handling.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains a unique <see cref="Id"/> for correlation and identification purposes.</item>
/// <item>Stores the timestamp <see cref="CreatedAt"/> when the event instance was created.</item>
/// <item>Holds a <see cref="Message"/> payload to demonstrate passing data through the event bus.</item>
/// <item>Can be used in sample modules, tests, or tutorials to illustrate event flow and middleware behavior.</item>
/// </list>
/// </remarks>
public class FluentExampleEvent(string message) : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = message;
}