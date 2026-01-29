namespace ModularityKit.Mutators.Abstractions.Metrics;

/// <summary>
/// Captures execution metrics for a single mutation operation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MutationMetrics"/> provides detailed performance and operational metrics for a mutation,
/// including timing, policy evaluations, state changes, memory usage, and caching behavior.
/// These metrics are typically collected by the Mutators framework for auditing, monitoring, and optimization purposes.
/// </para>
/// <para>
/// Key considerations:
/// <list type="bullet">
/// <item><see cref="ExecutionTime"/> measures the total duration of the mutation.</item>
/// <item><see cref="ValidationTime"/> measures time spent validating the mutation.</item>
/// <item><see cref="PolicyEvaluationTime"/> measures time spent evaluating policies.</item>
/// <item><see cref="ValidatedRules"/> and <see cref="EvaluatedPolicies"/> track the number of rules/policies processed.</item>
/// <item><see cref="ChangesCount"/> counts state changes applied.</item>
/// <item><see cref="StateSize"/> and <see cref="MemoryUsed"/> provide insights into resource consumption.</item>
/// <item><see cref="UsedCache"/> indicates if a cache was leveraged.</item>
/// <item><see cref="AdditionalMetrics"/> allows extensibility for custom metrics.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class MutationMetrics
{
    /// <summary>
    /// Timestamp when the mutation was recorded.
    /// </summary>
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Total execution time of the mutation.
    /// </summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// Time spent validating the mutation.
    /// </summary>
    public TimeSpan ValidationTime { get; init; }

    /// <summary>
    /// Time spent evaluating policies.
    /// </summary>
    public TimeSpan PolicyEvaluationTime { get; init; }

    /// <summary>
    /// Number of rules validated during the mutation.
    /// </summary>
    public int ValidatedRules { get; init; }

    /// <summary>
    /// Number of policies evaluated during the mutation.
    /// </summary>
    public int EvaluatedPolicies { get; init; }

    /// <summary>
    /// Number of state changes applied.
    /// </summary>
    public int ChangesCount { get; init; }

    /// <summary>
    /// Size of the state before the mutation (in bytes, if applicable).
    /// </summary>
    public long? StateSize { get; init; }

    /// <summary>
    /// Memory used during mutation execution (in bytes).
    /// </summary>
    public long? MemoryUsed { get; init; }

    /// <summary>
    /// Indicates whether a cache was used.
    /// </summary>
    public bool UsedCache { get; init; }

    /// <summary>
    /// Additional custom metrics.
    /// </summary>
    public IReadOnlyDictionary<string, object> AdditionalMetrics { get; init; }
        = new Dictionary<string, object>();
}
