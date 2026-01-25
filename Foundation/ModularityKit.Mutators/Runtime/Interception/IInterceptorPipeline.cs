using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Runtime.Interception;

/// <summary>
/// Represents pipeline of mutation interceptors that can observe and react to mutation execution events.
/// </summary>
/// <remarks>
/// <para>
/// Interceptors allow cross-cutting logic such as logging, auditing, metrics collection, or additional validations
/// to be executed before or after a mutation, or when a mutation fails or is blocked by a policy.
/// </para>
/// <para>
/// The pipeline manages multiple interceptors and ensures that all registered interceptors are invoked
/// in a deterministic order for each mutation lifecycle event.
/// </para>
/// </remarks>
public interface IInterceptorPipeline
{
    /// <summary>
    /// Registers an interceptor in the pipeline.
    /// </summary>
    /// <param name="interceptor">The interceptor to register.</param>
    void Register(IMutationInterceptor interceptor);

    /// <summary>
    /// Unregisters an interceptor from the pipeline by name.
    /// </summary>
    /// <param name="name">The name of the interceptor to remove.</param>
    void Unregister(string name);

    /// <summary>
    /// Invoked before a mutation is applied.
    /// </summary>
    /// <param name="intent">The mutation intent describing what the mutation will do.</param>
    /// <param name="context">The execution context of the mutation.</param>
    /// <param name="state">The current state before mutation.</param>
    /// <param name="executionId">Unique identifier for this mutation execution.</param>
    /// <param name="cancellationToken">Token for observing cancellation.</param>
    Task OnBeforeMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked after a mutation has been successfully applied.
    /// </summary>
    /// <param name="intent">The mutation intent describing what was executed.</param>
    /// <param name="context">The execution context of the mutation.</param>
    /// <param name="oldState">The state before mutation.</param>
    /// <param name="newState">The state after mutation.</param>
    /// <param name="changes">The set of changes applied during the mutation.</param>
    /// <param name="executionId">Unique identifier for this mutation execution.</param>
    /// <param name="cancellationToken">Token for observing cancellation.</param>
    Task OnAfterMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object? oldState,
        object? newState,
        ChangeSet changes,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked when a mutation fails due to an exception.
    /// </summary>
    /// <param name="intent">The mutation intent describing the attempted operation.</param>
    /// <param name="context">The execution context of the mutation.</param>
    /// <param name="state">The state at the time of failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="executionId">Unique identifier for this mutation execution.</param>
    /// <param name="cancellationToken">Token for observing cancellation.</param>
    Task OnMutationFailedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        Exception exception,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked when a mutation is blocked by a policy decision.
    /// </summary>
    /// <param name="intent">The mutation intent describing the attempted operation.</param>
    /// <param name="context">The execution context of the mutation.</param>
    /// <param name="state">The state at the time of policy evaluation.</param>
    /// <param name="decision">The policy decision that blocked the mutation.</param>
    /// <param name="executionId">Unique identifier for this mutation execution.</param>
    /// <param name="cancellationToken">Token for observing cancellation.</param>
    Task OnPolicyBlockedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        PolicyDecision decision,
        string executionId,
        CancellationToken cancellationToken = default);
}
