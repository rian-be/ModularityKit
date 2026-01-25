using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Exceptions;
using ModularityKit.Mutators.Abstractions.Results;
using ModularityExecutionContext = ModularityKit.Mutators.Abstractions.ExecutionContext;

namespace ModularityKit.Mutators.Runtime;

/// <summary>
/// Responsible for executing a single mutation against a given state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MutationExecutor"/> applies mutations in a synchronous fashion (the <see cref="IMutation{TState}.Apply"/>
/// method is executed inline) while respecting timeouts and cancellation tokens provided via
/// <see cref="System.Threading.ExecutionContext"/> and <paramref>
///     <name>cancellationToken</name>
/// </paramref>
/// .
/// </para>
/// <para>
/// This class does **not** perform policy checks, auditing, or interceptor pipelines — it is a low-level executor
/// used by <see cref="MutationEngine"/> to actually run mutations.
/// </para>
/// </remarks>
internal sealed class MutationExecutor : IMutationExecutor
{
    /// <summary>
    /// Executes a mutation against the provided state.
    /// </summary>
    /// <typeparam name="TState">The type of the state being mutated.</typeparam>
    /// <param name="mutation">The mutation instance to execute.</param>
    /// <param name="state">The current state on which the mutation will be applied.</param>
    /// <param name="context">
    /// Execution context containing metadata, cancellation, and timeout information.
    /// </param>
    /// <param name="cancellationToken">Optional token to observe cancellation requests.</param>
    /// <returns>The result of the mutation execution, including the new state and applied changes.</returns>
    /// <exception cref="ExecutionTimeoutException">
    /// Thrown if the mutation execution exceeds the configured <see cref="ModularityExecutionContext.Timeout"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the <paramref name="cancellationToken"/> is signaled before or during execution.
    /// </exception>
    public async Task<MutationResult<TState>> ExecuteAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        ModularityExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.IsTimedOut())
        {
            throw new ExecutionTimeoutException(
                context.Timeout!.Value,
                DateTimeOffset.UtcNow - context.StartedAt);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = mutation.Apply(state);
        return await Task.FromResult(result);
    }
}
