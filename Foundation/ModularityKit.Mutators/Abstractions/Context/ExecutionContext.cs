using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Runtime;

namespace ModularityKit.Mutators.Abstractions.Context;

/// <summary>
/// Represents the runtime context of a mutation execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ExecutionContext"/> encapsulates all runtime information for a single mutation execution.
/// It is used internally by <see cref="IMutationEngine"/> and <see cref="IMutationExecutor"/>
/// to manage execution state, timeouts, and cancellation.
/// </para>
/// <para>
/// Key properties include:
/// <list type="bullet">
///   <item><description><see cref="ExecutionId"/> — unique identifier for the mutation execution.</description></item>
///   <item><description><see cref="StartedAt"/> — timestamp when execution began.</description></item>
///   <item><description><see cref="Timeout"/> — optional maximum allowed duration for the mutation.</description></item>
///   <item><description><see cref="CancellationToken"/> — allows cooperative cancellation of execution.</description></item>
///   <item><description><see cref="Data"/> — dictionary for storing arbitrary contextual data during execution.</description></item>
/// </list>
/// </para>
/// <para>
/// Use <see cref="IsTimedOut"/> to check whether the mutation has exceeded the allowed <see cref="Timeout"/>.
/// </para>
/// </remarks>
internal sealed class ExecutionContext
{
    /// <summary>
    /// Unique identifier for this mutation execution.
    /// </summary>
    public string ExecutionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp indicating when execution started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional timeout for the mutation execution.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Cancellation token for cooperative cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Arbitrary additional contextual data.
    /// </summary>
    public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Checks whether the execution has exceeded the configured timeout.
    /// </summary>
    /// <returns>True if execution duration has exceeded <see cref="Timeout"/>; otherwise false.</returns>
    public bool IsTimedOut()
    {
        if (Timeout == null) return false;
        return DateTimeOffset.UtcNow - StartedAt > Timeout.Value;
    }
}
