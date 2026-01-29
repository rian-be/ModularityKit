using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Abstractions.Interception;

/// <summary>
/// Interceptor for mutation execution. Allows observing and reacting to mutations
/// at various stages: before execution, after execution, on failure, or when blocked by policy.
/// </summary>
/// <remarks>
/// Interceptors are executed in order based on the <see cref="Order"/> property (lower values run first).
/// They can be used for logging, auditing, validation, side effects, or monitoring mutation workflows.
/// </remarks>
public interface IMutationInterceptor
{
    /// <summary>
    /// Unique name of the interceptor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Execution order of the interceptor (lower numbers run first).
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Called before the mutation is executed.
    /// </summary>
    Task OnBeforeMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called after the mutation has successfully executed.
    /// </summary>
    Task OnAfterMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object? oldState,
        object? newState,
        ChangeSet changes,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the mutation execution fails with an exception.
    /// </summary>
    Task OnMutationFailedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        Exception exception,
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the mutation is blocked by a policy decision.
    /// </summary>
    Task OnPolicyBlockedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        PolicyDecision decision,
        string executionId,
        CancellationToken cancellationToken = default);
}
