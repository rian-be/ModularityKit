using ModularityKit.Mutators.Abstractions.Changes;

namespace ModularityKit.Mutators.Abstractions.History;

/// <summary>
/// Represents a single point in the timeline of a state change.
/// </summary>
/// <remarks>
/// Each <see cref="StateChangeTimeline"/> links a <see cref="StateChange"/> to its execution context,
/// including the mutation execution ID, the actor responsible, and an optional reason.
/// This allows tracing and auditing how a specific path or property in the state evolved over time.
/// </remarks>
public sealed class StateChangeTimeline
{
    /// <summary>
    /// Timestamp when the change occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The state change that occurred.
    /// </summary>
    public StateChange Change { get; init; } = null!;

    /// <summary>
    /// Unique identifier of the mutation execution that produced this change.
    /// </summary>
    public string ExecutionId { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the actor who triggered the mutation.
    /// </summary>
    public string? ActorId { get; init; }

    /// <summary>
    /// Optional human-readable reason for the change.
    /// </summary>
    public string? Reason { get; init; }
}
