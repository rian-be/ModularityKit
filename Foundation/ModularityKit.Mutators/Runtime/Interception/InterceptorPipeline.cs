using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Runtime.Interception;

/// <summary>
/// Pipeline responsible for sequential execution of mutation interceptors.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="InterceptorPipeline"/> manages registration, unregistration, and execution of interceptors
/// implementing <see cref="IMutationInterceptor"/>. Interceptors are sorted by their <c>Order</c> property
/// to ensure deterministic execution order.
/// </para>
/// <para>
/// The pipeline also filters interceptors via <see cref="MutationInterceptorBase.ShouldRun"/>. 
/// Method calls are executed asynchronously to integrate with the ModularityKit mutation pipeline.
/// </para>
/// </remarks>
public sealed class InterceptorPipeline : IInterceptorPipeline
{
    private readonly List<IMutationInterceptor> _interceptors = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Registers a new interceptor in the pipeline.
    /// </summary>
    /// <param name="interceptor">The interceptor to register. Cannot be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="interceptor"/> is <c>null</c>.</exception>
    /// <remarks>
    /// After adding, the interceptors list is sorted by <c>Order</c> to guarantee deterministic execution order.
    /// </remarks>
    public void Register(IMutationInterceptor interceptor)
    {
        ArgumentNullException.ThrowIfNull(interceptor);
        lock (_lock)
        {
            _interceptors.Add(interceptor);
            _interceptors.Sort((a, b) => a.Order.CompareTo(b.Order));
        }
    }

    /// <summary>
    /// Removes an interceptor from the pipeline by its name.
    /// </summary>
    /// <param name="name">The name of the interceptor to remove.</param>
    public void Unregister(string name)
    {
        lock (_lock) _interceptors.RemoveAll(i => i.Name == name);
    }

    /// <summary>
    /// Gets a snapshot of the currently registered interceptors.
    /// </summary>
    /// <returns>An array of interceptors.</returns>
    private IMutationInterceptor[] GetSnapshot()
    {
        lock (_lock) return _interceptors.ToArray();
    }

    /// <summary>
    /// Filters interceptors to determine which ones should run for a given mutation intent and context.
    /// </summary>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    /// <returns>An array of interceptors that should be executed.</returns>
    private IMutationInterceptor[] GetApplicable(MutationIntent intent, MutationContext context)
    {
        var snapshot = GetSnapshot();
        var result = new List<IMutationInterceptor>(snapshot.Length);

        foreach (var t in snapshot)
        {
            if (t is MutationInterceptorBase baseInt)
            {
                if (!baseInt.ShouldRun(intent, context))
                    continue;
            }

            result.Add(t);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Executes the given action on all applicable interceptors in the pipeline.
    /// </summary>
    /// <param name="action">The action to execute for each interceptor.</param>
    /// <param name="intent">The mutation intent.</param>
    /// <param name="context">The mutation context.</param>
    private async Task ExecuteAsync(Func<IMutationInterceptor, Task> action, MutationIntent intent, MutationContext context)
    {
        var interceptors = GetApplicable(intent, context);
        foreach (var t in interceptors)
        {
            await action(t);
        }
    }

    /// <inheritdoc/>
    public Task OnBeforeMutationAsync(MutationIntent intent, MutationContext context, object state, string executionId, CancellationToken cancellationToken = default)
        => ExecuteAsync(i => i.OnBeforeMutationAsync(intent, context, state, executionId, cancellationToken), intent, context);

    /// <inheritdoc/>
    public Task OnAfterMutationAsync(MutationIntent intent, MutationContext context, object? oldState, object? newState, ChangeSet changes, string executionId, CancellationToken cancellationToken = default)
        => ExecuteAsync(i => i.OnAfterMutationAsync(intent, context, oldState, newState, changes, executionId, cancellationToken), intent, context);

    /// <inheritdoc/>
    public Task OnMutationFailedAsync(MutationIntent intent, MutationContext context, object state, Exception exception, string executionId, CancellationToken cancellationToken = default)
        => ExecuteAsync(i => i.OnMutationFailedAsync(intent, context, state, exception, executionId, cancellationToken), intent, context);

    /// <inheritdoc/>
    public Task OnPolicyBlockedAsync(MutationIntent intent, MutationContext context, object state, PolicyDecision decision, string executionId, CancellationToken cancellationToken = default)
        => ExecuteAsync(i => i.OnPolicyBlockedAsync(intent, context, state, decision, executionId, cancellationToken), intent, context);
}