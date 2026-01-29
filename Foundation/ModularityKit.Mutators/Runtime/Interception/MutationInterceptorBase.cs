using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Runtime.Interception;

/// <summary>
/// Base class for mutation interceptors providing default, no-op implementations of <see cref="IMutationInterceptor"/> methods.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MutationInterceptorBase"/> can be inherited to create custom interceptors for the mutation pipeline.
/// By default, all methods do nothing and <see cref="ShouldRun"/> returns <c>true</c>, meaning the interceptor
/// executes for all mutation intents and contexts unless overridden.
/// </para>
/// <para>
/// Subclasses can override <see cref="Order"/> to control execution sequence and <see cref="Name"/> to provide
/// a meaningful identifier for logging or diagnostics.
/// </para>
/// </remarks>
public abstract class MutationInterceptorBase : IMutationInterceptor
{
    /// <summary>
    /// Gets the name of the interceptor. Defaults to the type name.
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Gets the execution order of the interceptor. Lower numbers run first. Defaults to 0.
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Determines whether this interceptor should run for a given mutation intent and context.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <returns><c>true</c> if the interceptor should run; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Override to implement conditional execution logic. By default, this returns <c>true</c> for all cases.
    /// </remarks>
    protected internal virtual bool ShouldRun(MutationIntent intent, MutationContext context)
        => true;

    /// <summary>
    /// Called before a mutation is applied.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <param name="state">The current state object before mutation.</param>
    /// <param name="executionId">A unique execution identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnBeforeMutationAsync(MutationIntent intent, MutationContext context, object state, string executionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after a mutation has been applied.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <param name="oldState">The state before the mutation.</param>
    /// <param name="newState">The state after the mutation.</param>
    /// <param name="changes">The set of changes produced by the mutation.</param>
    /// <param name="executionId">A unique execution identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnAfterMutationAsync(MutationIntent intent, MutationContext context, object? oldState, object? newState, ChangeSet changes, string executionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called when a mutation fails with an exception.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <param name="state">The current state object at the time of failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="executionId">A unique execution identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnMutationFailedAsync(MutationIntent intent, MutationContext context, object state, Exception exception, string executionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called when a mutation is blocked by a policy.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <param name="state">The current state object at the time of policy evaluation.</param>
    /// <param name="decision">The policy decision that blocked the mutation.</param>
    /// <param name="executionId">A unique execution identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnPolicyBlockedAsync(MutationIntent intent, MutationContext context, object state, PolicyDecision decision, string executionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}