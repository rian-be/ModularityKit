using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Results;

namespace ModularityKit.Mutators.Abstractions;

/// <summary>
/// Represents the result of executing a batch of mutations as a single operation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BatchMutationResult{TState}"/> aggregates the outcomes of multiple <see cref="IMutation{TState}"/> executions.
/// It contains the final state, individual results, aggregated changes, and summary statistics.
/// </para>
/// <para>
/// Key properties:
/// <list type="bullet">
///   <item><description><see cref="IsSuccess"/> — indicates if all mutations succeeded.</description></item>
///   <item><description><see cref="FinalState"/> — resulting state after applying all mutations.</description></item>
///   <item><description><see cref="Results"/> — list of individual <see cref="MutationResult{TState}"/> entries.</description></item>
///   <item><description><see cref="AggregatedChanges"/> — combined <see cref="ChangeSet"/> of all mutations.</description></item>
///   <item><description><see cref="SuccessCount"/> / <see cref="FailureCount"/> — counts of successful and failed mutations.</description></item>
///   <item><description><see cref="TotalExecutionTime"/> — cumulative execution time of the batch.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class BatchMutationResult<TState>
{
    /// <summary>
    /// Indicates whether all mutations in the batch succeeded.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Final state after applying all mutations.
    /// </summary>
    public TState? FinalState { get; init; }

    /// <summary>
    /// Individual results for each mutation in the batch.
    /// </summary>
    public IReadOnlyList<MutationResult<TState>> Results { get; init; } = [];

    /// <summary>
    /// Aggregated changes from all mutations.
    /// </summary>
    public ChangeSet AggregatedChanges { get; init; } = ChangeSet.Empty;

    /// <summary>
    /// Number of successful mutations.
    /// </summary>
    public int SuccessCount => Results.Count(r => r.IsSuccess);

    /// <summary>
    /// Number of failed mutations.
    /// </summary>
    public int FailureCount => Results.Count(r => !r.IsSuccess);

    /// <summary>
    /// Total elapsed time for executing the batch.
    /// </summary>
    public TimeSpan TotalExecutionTime { get; init; }
}
