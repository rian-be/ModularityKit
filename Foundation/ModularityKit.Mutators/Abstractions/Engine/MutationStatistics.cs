using System;

namespace ModularityKit.Mutators.Abstractions.Engine;

/// <summary>
/// Retrieves aggregated mutation execution statistics.
/// </summary>
public sealed class MutationStatistics
{
    /// <summary>
    /// Total number of executed mutations in the aggregation period.
    /// </summary>
    public long TotalExecuted { get; init; }

    /// <summary>
    /// Average execution time of mutations.
    /// </summary>
    public TimeSpan AverageExecutionTime { get; init; }

    /// <summary>
    /// Median (P50) execution time.
    /// </summary>
    public TimeSpan MedianExecutionTime { get; init; }

    /// <summary>
    /// 95th percentile execution time.
    /// </summary>
    public TimeSpan P95ExecutionTime { get; init; }

    /// <summary>
    /// Timestamp of the last statistics update.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; init; }
}
