namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when mutation execution exceeds the allowed timeout.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ExecutionTimeoutException"/> occurs when the mutation takes longer than the configured
/// <see cref="Timeout"/> to complete. This helps enforce SLA and system responsiveness.
/// </para>
/// <para>
/// <see cref="Elapsed"/> provides the actual duration the mutation ran before timing out.
/// </para>
/// </remarks>
/// <param name="timeout">The maximum allowed duration for the mutation.</param>
/// <param name="elapsed">The actual duration the mutation ran before timing out.</param>
public sealed class ExecutionTimeoutException(TimeSpan timeout, TimeSpan elapsed) : MutationException($"Mutation execution timed out after {elapsed.TotalSeconds}s (timeout: {timeout.TotalSeconds}s)")
{
    /// <summary>
    /// The configured timeout for the mutation execution.
    /// </summary>
    public TimeSpan Timeout { get; } = timeout;

    /// <summary>
    /// The actual elapsed time before the timeout occurred.
    /// </summary>
    public TimeSpan Elapsed { get; } = elapsed;
}
