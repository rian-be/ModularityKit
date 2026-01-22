namespace ModularityKit.Mutators.Abstractions.Metrics;

/// <summary>
/// Represents a scoped metrics collection for a single mutation execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IMetricsScope"/> is used to track metrics related to a specific mutation,
/// such as execution time, validation time, policy evaluation time, state size, and memory usage.
/// It also allows recording custom metrics and building a final <see cref="MutationMetrics"/> object.
/// </para>
/// <para>
/// A typical workflow:
/// <list type="bullet">
/// <item>Obtain a scope from <see cref="IMetricsCollector.BeginScope"/>.</item>
/// <item>Record individual metrics during mutation execution using the provided methods.</item>
/// <item>Call <see cref="Build"/> to get the <see cref="MutationMetrics"/> object representing the collected data.</item>
/// <item>Dispose the scope after recording is complete.</item>
/// </list>
/// </para>
/// </remarks>
public interface IMetricsScope : IDisposable
{
    /// <summary>
    /// Unique identifier for the mutation execution associated with this scope.
    /// </summary>
    string ExecutionId { get; }

    /// <summary>
    /// Records the time spent validating the mutation.
    /// </summary>
    /// <param name="time">Duration of validation.</param>
    void RecordValidationTime(TimeSpan time);

    /// <summary>
    /// Records the time spent evaluating policies for the mutation.
    /// </summary>
    /// <param name="time">Duration of policy evaluation.</param>
    void RecordPolicyEvaluationTime(TimeSpan time);

    /// <summary>
    /// Records the size of the state before or after mutation (in bytes).
    /// </summary>
    /// <param name="bytes">State size in bytes.</param>
    void RecordStateSize(long bytes);

    /// <summary>
    /// Records memory usage during mutation execution (in bytes).
    /// </summary>
    /// <param name="bytes">Memory used in bytes.</param>
    void RecordMemoryUsage(long bytes);

    /// <summary>
    /// Adds a custom metric with a key and value to the scope.
    /// </summary>
    /// <param name="key">Metric key/name.</param>
    /// <param name="value">Metric value.</param>
    void AddCustomMetric(string key, object value);

    /// <summary>
    /// Builds the <see cref="MutationMetrics"/> object representing all collected metrics in this scope.
    /// </summary>
    /// <returns>A <see cref="MutationMetrics"/> instance.</returns>
    MutationMetrics Build();
}
