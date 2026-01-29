namespace ModularityKit.Mutators.Abstractions.Metrics;

/// <summary>
/// Represents aggregated metrics over a period of mutation executions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AggregatedMetrics"/> provides statistical insights on mutation performance,
/// including execution time percentiles, throughput, and total mutation count. It is typically
/// obtained via <see cref="IMetricsCollector.GetAggregatedAsync"/>.
/// </para>
/// <para>
/// Key metrics include:
/// <list type="bullet">
/// <item><see cref="TotalMutations"/> - total number of mutations executed in the period.</item>
/// <item><see cref="AverageExecutionTime"/> - mean duration of mutations.</item>
/// <item><see cref="MinExecutionTime"/> / <see cref="MaxExecutionTime"/> - minimum and maximum execution times.</item>
/// <item>P50 / P95 / P99 percentiles - statistical distribution of execution durations.</item>
/// <item><see cref="ThroughputPerSecond"/> - average number of mutations per second.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class AggregatedMetrics
{
    /// <summary>
    /// Start of the aggregation period.
    /// </summary>
    public DateTimeOffset From { get; init; }

    /// <summary>
    /// End of the aggregation period.
    /// </summary>
    public DateTimeOffset To { get; init; }

    /// <summary>
    /// Total number of mutations executed in the period.
    /// </summary>
    public int TotalMutations { get; init; }

    /// <summary>
    /// Average execution time of mutations.
    /// </summary>
    public TimeSpan AverageExecutionTime { get; init; }

    /// <summary>
    /// Minimum execution time recorded.
    /// </summary>
    public TimeSpan MinExecutionTime { get; init; }

    /// <summary>
    /// Maximum execution time recorded.
    /// </summary>
    public TimeSpan MaxExecutionTime { get; init; }

    /// <summary>
    /// 50th percentile execution time (median).
    /// </summary>
    public TimeSpan P50ExecutionTime { get; init; }

    /// <summary>
    /// 95th percentile execution time.
    /// </summary>
    public TimeSpan P95ExecutionTime { get; init; }

    /// <summary>
    /// 99th percentile execution time.
    /// </summary>
    public TimeSpan P99ExecutionTime { get; init; }

    /// <summary>
    /// Average throughput of mutations per second.
    /// </summary>
    public double ThroughputPerSecond { get; init; }
}
