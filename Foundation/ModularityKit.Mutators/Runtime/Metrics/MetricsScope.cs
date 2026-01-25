using System.Diagnostics;
using ModularityKit.Mutators.Abstractions.Metrics;

namespace ModularityKit.Mutators.Runtime.Metrics;

/// <summary>
/// Represents scoped collector for mutation metrics during a single execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MetricsScope"/> allows measuring and recording various metrics for a mutation execution,
/// including execution time, validation time, policy evaluation time, state size, memory usage, and custom metrics.
/// </para>
/// <para>
/// Metrics are collected in-memory and finalized via <see cref="Build"/> to produce a <see cref="MutationMetrics"/> instance.
/// A <see cref="Stopwatch"/> is used to track total execution time.
/// </para>
/// <para>
/// This class implements <see cref="IMetricsScope"/> and supports <see cref="IDisposable"/> for optional cleanup.
/// </para>
/// </remarks>
internal sealed class MetricsScope(string executionId) : IMetricsScope
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly Dictionary<string, object> _customMetrics = new();

    private TimeSpan? _validationTime;
    private TimeSpan? _policyEvaluationTime;
    private long? _stateSize;
    private long? _memoryUsage;

    /// <summary>
    /// Gets the unique execution identifier associated with this scope.
    /// </summary>
    public string ExecutionId { get; } = executionId;

    /// <summary>
    /// Records the time spent in mutation validation.
    /// </summary>
    /// <param name="time">Duration of validation.</param>
    public void RecordValidationTime(TimeSpan time) =>
        _validationTime = time;

    /// <summary>
    /// Records the time spent evaluating policies for the mutation.
    /// </summary>
    /// <param name="time">Duration of policy evaluation.</param>
    public void RecordPolicyEvaluationTime(TimeSpan time) =>
        _policyEvaluationTime = time;

    /// <summary>
    /// Records the size of the state object involved in the mutation.
    /// </summary>
    /// <param name="bytes">State size in bytes.</param>
    public void RecordStateSize(long bytes) =>
        _stateSize = bytes;

    /// <summary>
    /// Records memory usage during the mutation execution.
    /// </summary>
    /// <param name="bytes">Memory used in bytes.</param>
    public void RecordMemoryUsage(long bytes) =>
        _memoryUsage = bytes;  

    /// <summary>
    /// Adds a custom metric key/value pair to this scope.
    /// </summary>
    /// <param name="key">The name of the custom metric.</param>
    /// <param name="value">The value of the custom metric.</param>
    public void AddCustomMetric(string key, object value) =>
        _customMetrics[key] = value;

    /// <summary>
    /// Finalizes and builds a <see cref="MutationMetrics"/> object with all recorded metrics.
    /// </summary>
    /// <returns>A <see cref="MutationMetrics"/> instance containing all metrics for this execution.</returns>
    public MutationMetrics Build()
    {
        _stopwatch.Stop();

        return new MutationMetrics
        {
            ExecutionTime = _stopwatch.Elapsed,
            ValidationTime = _validationTime ?? TimeSpan.Zero,
            PolicyEvaluationTime = _policyEvaluationTime ?? TimeSpan.Zero,
            StateSize = _stateSize,
            MemoryUsed = _memoryUsage,
            AdditionalMetrics = _customMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    /// <summary>
    /// Disposes the metrics scope, allowing for optional cleanup.
    /// </summary>
    public void Dispose()
    {
        // todo: Cleanup Dispose in MetricsScope
    }
}
