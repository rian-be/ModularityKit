using System.Diagnostics;
using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Audit;
using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Exceptions;
using ModularityKit.Mutators.Abstractions.History;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Abstractions.Metrics;
using ModularityKit.Mutators.Abstractions.Policies;
using ModularityKit.Mutators.Abstractions.Results;
using ModularityKit.Mutators.Runtime.Interception;
using ModularityKit.Mutators.Runtime.Policies;
using ModularityExecutionContext = ModularityKit.Mutators.Abstractions.ExecutionContext;

namespace ModularityKit.Mutators.Runtime;

internal sealed class MutationEngine(
    IMutationExecutor executor,
    IPolicyRegistry policyRegistry,
    IInterceptorPipeline interceptorPipeline,
    IMutationAuditor auditor,
    IMutationHistoryStore historyStore,
    IMetricsCollector metricsCollector,
    MutationEngineOptions options)
    : IMutationEngine
{
    private readonly IMutationExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    private readonly IPolicyRegistry _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
    private readonly IInterceptorPipeline _interceptorPipeline = interceptorPipeline ?? throw new ArgumentNullException(nameof(interceptorPipeline));
    private readonly IMutationAuditor _auditor = auditor ?? throw new ArgumentNullException(nameof(auditor));
    private readonly IMutationHistoryStore _historyStore = historyStore ?? throw new ArgumentNullException(nameof(historyStore));
    private readonly IMetricsCollector _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
    private readonly MutationEngineOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public async Task<MutationResult<TState>> ExecuteAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        CancellationToken cancellationToken = default)
    {
        var executionId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();
        var metricsScope = _metricsCollector.BeginScope(executionId);

        try
        {
            await _interceptorPipeline.OnBeforeMutationAsync(
                mutation.Intent,
                mutation.Context,
                state,
                executionId,
                cancellationToken);

            var policyDecision = await EvaluatePoliciesAsync(mutation, state, cancellationToken);
            metricsScope.RecordPolicyEvaluationTime(stopwatch.Elapsed);

            if (!policyDecision.IsAllowed)
            {
                var result = MutationResult<TState>.PolicyBlocked(policyDecision);

                await _interceptorPipeline.OnPolicyBlockedAsync(
                    mutation.Intent,
                    mutation.Context,
                    state,
                    policyDecision,
                    executionId,
                    cancellationToken);

                await AuditFailureAsync(mutation, state, result, executionId, stopwatch.Elapsed);

                return result;
            }
            
            if (mutation.Context.Mode != MutationMode.Commit || _options.AlwaysValidate)
            {
                var validationStart = stopwatch.Elapsed;
                var validation = mutation.Validate(state);
                metricsScope.RecordValidationTime(stopwatch.Elapsed - validationStart);

                if (!validation.IsValid)
                {
                    var result = MutationResult<TState>.Failure(validation);
                    await AuditFailureAsync(mutation, state, result, executionId, stopwatch.Elapsed);
                    return result;
                }
            }
            
            MutationResult<TState> mutationResult;
            var executionContext = new ModularityExecutionContext
            {
                ExecutionId = executionId,
                Timeout = _options.ExecutionTimeout,
                CancellationToken = cancellationToken
            };

            switch (mutation.Context.Mode)
            {
                case MutationMode.Simulate:
                    mutationResult = mutation.Simulate(state);
                    break;

                case MutationMode.Validate:
                    var validation = mutation.Validate(state);
                    mutationResult = validation.IsValid
                        ? MutationResult<TState>.Success(state, ChangeSet.Empty)
                        : MutationResult<TState>.Failure(validation);
                    break;

                case MutationMode.Commit:
                default:
                    mutationResult = await _executor.ExecuteAsync(
                        mutation,
                        state,
                        executionContext,
                        cancellationToken);
                    break;
            }

            stopwatch.Stop();

            if (policyDecision.Modifications != null && mutationResult.IsSuccess)
                mutationResult = ApplyPolicyModifications(mutationResult, policyDecision.Modifications);
            
            await _interceptorPipeline.OnAfterMutationAsync(
                mutation.Intent,
                mutation.Context,
                state,
                mutationResult.NewState,
                mutationResult.Changes,
                executionId,
                cancellationToken);

            await AuditSuccessAsync(
                mutation,
                state,
                mutationResult,
                policyDecision,
                executionId,
                stopwatch.Elapsed);
            
            if (mutationResult.IsSuccess && mutation.Context.Mode == MutationMode.Commit)
            {
                await StoreInHistoryAsync(
                    mutation,
                    mutationResult,
                    executionId,
                    stopwatch.Elapsed,
                    cancellationToken);
            }
            
            metricsScope.RecordStateSize(EstimateStateSize(state));
            var metrics = metricsScope.Build();
            await _metricsCollector.RecordAsync(executionId, metrics, cancellationToken);

            return mutationResult;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await _interceptorPipeline.OnMutationFailedAsync(
                mutation.Intent,
                mutation.Context,
                state,
                ex,
                executionId,
                cancellationToken);

            await AuditExceptionAsync(mutation, state, ex, executionId, stopwatch.Elapsed);

            throw new MutationException(
                $"Mutation execution failed: {ex.Message}",
                ex)
            {
                ExecutionId = executionId
            };
        }
        finally
        {
            metricsScope.Dispose();
        }
    }

    public async Task<BatchMutationResult<TState>> ExecuteBatchAsync<TState>(
        IEnumerable<IMutation<TState>> mutations,
        TState state,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<MutationResult<TState>>();
        var currentState = state;
        var allChanges = new ChangeSet();

        foreach (var mutation in mutations)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await ExecuteAsync(mutation, currentState, cancellationToken);
            results.Add(result);

            if (!result.IsSuccess)
            {
                if (_options.StopBatchOnFirstFailure)
                    break;
                continue;
            }

            currentState = result.NewState!;

            foreach (var change in result.Changes.Changes)
            {
                allChanges.Add(change);
            }
        }

        stopwatch.Stop();

        var allSucceeded = results.All(r => r.IsSuccess);

        return new BatchMutationResult<TState>
        {
            IsSuccess = allSucceeded,
            FinalState = allSucceeded ? currentState : default,
            Results = results,
            AggregatedChanges = allChanges,
            TotalExecutionTime = stopwatch.Elapsed
        };
    }

    public void RegisterPolicy<TState>(IMutationPolicy<TState> policy) =>
        _policyRegistry.Register(policy);

    public void RegisterInterceptor(IMutationInterceptor interceptor) =>
        _interceptorPipeline.Register(interceptor);

    public async Task<MutationHistory> GetHistoryAsync(string stateId, CancellationToken cancellationToken = default) =>
        await _historyStore.GetHistoryAsync(stateId, cancellationToken);

    public async Task<MutationStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var metrics = await _metricsCollector.GetAggregatedAsync(
            now.AddDays(-30),
            now,
            cancellationToken);

        return new MutationStatistics
        {
            TotalExecuted = metrics.TotalMutations,
            AverageExecutionTime = metrics.AverageExecutionTime,
            MedianExecutionTime = metrics.P50ExecutionTime,
            P95ExecutionTime = metrics.P95ExecutionTime,
            LastUpdatedAt = now
        };
    }
    

    private Task<PolicyDecision> EvaluatePoliciesAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        CancellationToken cancellationToken)
    {
        var policies = _policyRegistry.GetPolicies<TState>();

        foreach (var policy in policies.OrderByDescending(p => p.Priority))
        {
            var decision = policy.Evaluate(mutation, state);

            if (!decision.IsAllowed)
                return Task.FromResult(decision);

            if (decision.Modifications != null)
                return Task.FromResult(decision);
        }

        return Task.FromResult(PolicyDecision.Allow());
    }

    private MutationResult<TState> ApplyPolicyModifications<TState>(
        MutationResult<TState> result,
        IReadOnlyDictionary<string, object> modifications)
    {
        // Policy modifications can add metadata, side effects, etc.
        // This is extensible based on specific needs
        return result;
    }

    private async Task AuditSuccessAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        MutationResult<TState> result,
        PolicyDecision policyDecision,
        string executionId,
        TimeSpan duration)
    {
        var entry = new MutationAuditEntry
        {
            ExecutionId = executionId,
            StateType = typeof(TState).Name,
            MutationIntent = mutation.Intent,
            Context = mutation.Context,
            Changes = result.Changes,
            IsSuccess = true,
            PolicyDecisions = [policyDecision],
            Timestamp = mutation.Context.Timestamp,
            Duration = duration,
            SourceIpAddress = mutation.Context.SourceIpAddress,
            UserAgent = mutation.Context.UserAgent
        };

        await _auditor.AuditAsync(entry);
    }

    private async Task AuditFailureAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        MutationResult<TState> result,
        string executionId,
        TimeSpan duration)
    {
        var entry = new MutationAuditEntry
        {
            ExecutionId = executionId,
            StateType = typeof(TState).Name,
            MutationIntent = mutation.Intent,
            Context = mutation.Context,
            Changes = result.Changes,
            IsSuccess = false,
            ErrorMessage = string.Join("; ", result.ValidationResult.Errors.Select(e => e.Message)),
            Timestamp = mutation.Context.Timestamp,
            Duration = duration
        };

        await _auditor.AuditAsync(entry);
    }

    private async Task AuditExceptionAsync<TState>(
        IMutation<TState> mutation,
        TState state,
        Exception exception,
        string executionId,
        TimeSpan duration)
    {
        var entry = new MutationAuditEntry
        {
            ExecutionId = executionId,
            StateType = typeof(TState).Name,
            MutationIntent = mutation.Intent,
            Context = mutation.Context,
            IsSuccess = false,
            ErrorMessage = exception.Message,
            Timestamp = mutation.Context.Timestamp,
            Duration = duration
        };

        await _auditor.AuditAsync(entry);
    }

    private async Task StoreInHistoryAsync<TState>(
        IMutation<TState> mutation,
        MutationResult<TState> result,
        string executionId,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        var entry = new MutationHistoryEntry
        {
            ExecutionId = executionId,
            Intent = mutation.Intent,
            Context = mutation.Context,
            Changes = result.Changes,
            SideEffects = result.SideEffects.ToList(),
            Timestamp = mutation.Context.Timestamp,
            ExecutionTime = duration
        };

        await _historyStore.StoreAsync(entry, cancellationToken);
    }

    private long EstimateStateSize(object state)
    {
        //todo: Simple estimation - in real implementation would use proper serialization
        return 1024; // Placeholder
    }
}

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
