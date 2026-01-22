namespace ModularityKit.Mutators.Abstractions.Metrics;

/// <summary>
/// Collector interface for capturing and recording mutation metrics.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IMetricsCollector"/> defines the contract for recording, scoping, and aggregating
/// metrics produced during mutation execution. Implementations may store metrics in memory,
/// database, or telemetry systems, allowing analysis of performance, policy evaluation, and state changes.
/// </para>
/// <para>
/// Typical usage:
/// <list type="bullet">
/// <item>Begin a scoped collection for a mutation using <see cref="BeginScope"/>.</item>
/// <item>Record metrics using <see cref="RecordAsync"/>.</item>
/// <item>Retrieve aggregated metrics over a time period using <see cref="GetAggregatedAsync"/>.</item>
/// </list>
/// </para>
/// </remarks>
public interface IMetricsCollector
{
    /// <summary>
    /// Begins a metrics collection scope for a given mutation execution.
    /// </summary>
    /// <param name="executionId">Unique identifier of the mutation execution.</param>
    /// <returns>An <see cref="IMetricsScope"/> which should be disposed when metrics collection ends.</returns>
    IMetricsScope BeginScope(string executionId);

    /// <summary>
    /// Records metrics for a specific mutation execution.
    /// </summary>
    /// <param name="executionId">Unique identifier of the mutation execution.</param>
    /// <param name="metrics">Metrics to record.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task RecordAsync(
        string executionId,
        MutationMetrics metrics,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves aggregated metrics over a specified time range.
    /// </summary>
    /// <param name="from">Start of the aggregation period.</param>
    /// <param name="to">End of the aggregation period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="AggregatedMetrics"/> object containing aggregated results.</returns>
    Task<AggregatedMetrics> GetAggregatedAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
