using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Abstractions.Policies;
using ModularityKit.Mutators.Runtime.Interception;

namespace ModularityKit.Mutators.Runtime.Loggers;

/// <summary>
/// Simple console logger interceptor for mutation operations.
/// </summary>
/// <remarks>
/// <para>
/// Logs all mutation lifecycle events (before, after, failed, policy blocked) to the console.
/// Intended for development, testing, and debugging purposes.
/// </para>
/// <para>
/// Thread safe: the interceptor only writes to <see cref="Console"/>, which is internally synchronized by .NET.
/// </para>
/// <para>
/// Usage: register with <see cref="InterceptorPipeline"/> or via DI:
/// <code>
/// services.AddSingleton&lt;IMutationInterceptor, LoggingInterceptor&gt;();
/// </code>
/// </para>
/// </remarks>
internal sealed class LoggingInterceptor : MutationInterceptorBase
{
    /// <inheritdoc/>
    public override string Name => "LoggingInterceptor";

    /// <inheritdoc/>
    public override int Order => 0;

    protected internal override bool ShouldRun(MutationIntent intent, MutationContext context) => true;
    
    /// <inheritdoc/>
    public override Task OnBeforeMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        string executionId,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Before] {intent.OperationName} by {context.ActorId} (ExecutionId: {executionId})");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task OnAfterMutationAsync(
        MutationIntent intent,
        MutationContext context,
        object? oldState,
        object? newState,
        ChangeSet changes,
        string executionId,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[After] {intent.OperationName}, changes: {changes.Changes.Count} (ExecutionId: {executionId})");
        foreach (var change in changes.Changes)
        {
            Console.WriteLine($"  - {change.Path}: {change.OldValue} -> {change.NewValue}");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task OnMutationFailedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        Exception exception,
        string executionId,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Failed] {intent.OperationName} by {context.ActorId}: {exception.Message} (ExecutionId: {executionId})");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task OnPolicyBlockedAsync(
        MutationIntent intent,
        MutationContext context,
        object state,
        PolicyDecision decision,
        string executionId,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Blocked] {intent.OperationName} by {context.ActorId}: {decision.Reason} (ExecutionId: {executionId})");
        return Task.CompletedTask;
    }
}