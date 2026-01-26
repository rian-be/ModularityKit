using ModularityKit.Mutators.Abstractions.Results;
using ExecutionContext = ModularityKit.Mutators.Abstractions.Context.ExecutionContext;

namespace ModularityKit.Mutators.Abstractions.Engine;

/// <summary>
/// Low-level executor responsible for applying a mutation to a state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IMutationExecutor"/> performs the actual state transition defined
/// by a mutation. It operates after all governance concerns have been resolved
/// by the mutation engine (policy evaluation, validation, interception).
/// </para>
/// <para>
/// The executor is intentionally minimal and must remain free of:
/// </para>
/// <list type="bullet">
///   <item><description>Policy evaluation</description></item>
///   <item><description>Validation logic</description></item>
///   <item><description>Audit or history persistence</description></item>
/// </list>
/// <para>
/// Its sole responsibility is deterministic state transformation.
/// </para>
/// </remarks>
internal interface IMutationExecutor
{
    /// <summary>
    /// Executes a mutation against the provided state.
    /// </summary>
    /// <typeparam name="TState">The type of the state being mutated.</typeparam>
    /// <param name="mutation">The mutation to execute.</param>
    /// <param name="state">The current state.</param>
    /// <param name="context">The execution context for the mutation.</param>
    /// <param name="cancellationToken">Token used to cancel execution.</param>
    /// <returns>
    /// A <see cref="MutationResult{TState}"/> containing the resulting state
    /// and the computed change set.
    /// </returns>
    Task<MutationResult<TState>> ExecuteAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
