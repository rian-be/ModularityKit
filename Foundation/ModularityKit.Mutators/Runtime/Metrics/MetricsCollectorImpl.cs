using ModularityKit.Mutators.Abstractions.Metrics;

namespace ModularityKit.Mutators.Runtime.Metrics;

/// <summary>
/// Implementation of <see cref="MetricsCollectorImpl"/> responsible for collecting and aggregating mutation metrics.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BeginScope"/> stores per execution mutation metrics in memory and provides methods
/// to start scoped metrics recording (<see cref="GetAggregatedAsync"/>) and aggregate metrics over a time range
/// (<see cref="IMetricsCollector"/>).
/// </para>
/// <para>
/// Thread-safe access is guaranteed via an internal lock. All methods are asynchronous to integrate with
/// mutation pipelines, although internal operations are in-memory and complete synchronously.
/// </para>
/// </remarks>
internal sealed class MetricsCollectorImpl : IMetricsCollector
{
    private readonly Dictionary<string, MutationMetrics> _metrics = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Begins a new metrics scope for a mutation execution.
    /// </summary>
    /// <param name="executionId">A unique identifier for the mutation execution.</param>
    /// <returns>An <see cref="IMetricsScope"/> that can be used to record metrics within this execution scope.</returns>
    public IMetricsScope BeginScope(string executionId)
    {
        return new MetricsScope(executionId);
    }

    /// <summary>
    /// Records mutation metrics for a given execution.
    /// </summary>
    /// <param name="executionId">The unique execution identifier.</param>
    /// <param name="metrics">The <see cref="MutationMetrics"/> to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// If metrics for the given execution ID already exist, they are overwritten.
    /// </remarks>
    public Task RecordAsync(
        string executionId,
        MutationMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _metrics[executionId] = metrics;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Aggregates recorded metrics over the specified time range.
    /// </summary>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="AggregatedMetrics"/> object containing summary statistics.</returns>
    /// <remarks>
    /// Aggregation includes total mutations, average execution time, minimum, maximum, and percentile
    /// metrics (P50, P95, P99). If no metrics are recorded, returns an empty <see cref="AggregatedMetrics"/> with
    /// only the <c>From</c> and <c>To</c> timestamps populated.
    /// </remarks>
    public Task<AggregatedMetrics> GetAggregatedAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filteredMetrics = _metrics.Values
                .Where(m => m.RecordedAt >= from && m.RecordedAt <= to)
                .ToList();

            if (filteredMetrics.Count == 0)
            {
                return Task.FromResult(new AggregatedMetrics
                {
                    From = from,
                    To = to
                });
            }

            var executionTimes = filteredMetrics.Select(m => m.ExecutionTime).OrderBy(t => t).ToList();
            var durationSeconds = (to - from).TotalSeconds;
            var throughput = durationSeconds > 0 ? filteredMetrics.Count / durationSeconds : 0;

            return Task.FromResult(new AggregatedMetrics
            {
                From = from,
                To = to,
                TotalMutations = filteredMetrics.Count,
                AverageExecutionTime = TimeSpan.FromMilliseconds(
                    executionTimes.Average(t => t.TotalMilliseconds)),
                MinExecutionTime = executionTimes.First(),
                MaxExecutionTime = executionTimes.Last(),
                P50ExecutionTime = executionTimes[executionTimes.Count / 2],
                P95ExecutionTime = executionTimes[(int)(executionTimes.Count * 0.95)],
                P99ExecutionTime = executionTimes[(int)(executionTimes.Count * 0.99)],
                ThroughputPerSecond = throughput
            });
        }
    }
}