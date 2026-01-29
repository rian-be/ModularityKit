using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using ModularityKit.Mutators.Abstractions.Results;
using ModularityKit.Mutators.Runtime.Loggers;
using Mutators.Examples.FeatureFlags.Mutations;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Scenarios;

/// <summary>
/// BatchFeatureToggleScenario
/// 
/// Demonstrates batch execution of multiple feature flag mutations, both enabling and disabling features,
/// using the <see cref="IMutationEngine"/> and <see cref="IMutation{TState}"/> abstractions.
///
/// This scenario exercises:
/// 
/// - Batch mutation execution with <see cref="IMutationEngine.ExecuteBatchAsync"/>
/// - Enforcement of policies via <see cref="PolicyDecision"/>
/// - User level <see cref="MutationContext"/> for correlation, auditing, and approvals
/// - Logging of batch results using <see cref="MutationResultLogger"/>
/// - Handling multiple feature flags simultaneously, simulating real-world operations like releases or A/B tests
/// 
/// Key Steps:
/// 1. Initialize <see cref="FeatureFlagsState"/> with example feature flags.
/// 2. Construct a set of <see cref="EnableFeatureMutation"/> and <see cref="DisableFeatureMutation"/> instances,
///    each with its own <see cref="MutationContext"/>.
/// 3. Execute the batch using <see cref="IMutationEngine.ExecuteBatchAsync"/>.
/// 4. Log individual mutation results and policy decisions.
/// 5. Update local state using <see cref="BatchMutationResult{TState}.FinalState"/>.
/// 6. Output the final feature flag states.
///
/// Example Use Case:
/// - Simultaneously enabling/disabling multiple features during release.
/// - Performing controlled feature toggling for A/B testing or canary releases.
/// - Ensuring auditability and policy compliance across multiple feature mutations.
/// 
/// Notes:
/// - If any mutation is blocked by a policy, the other mutations in the batch are still executed.
/// - Correlation IDs and metadata are used to track approvals and reasons for audit purposes.
/// </summary>
internal static class BatchFeatureToggleScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Batch Feature Toggle Scenario ===");

        var state = new FeatureFlagsState
        {
            Flags = new Dictionary<string, bool>
            {
                ["LegacyCheckout"] = true,
                ["NewCheckout"] = false,
                ["DarkMode"] = false,
                ["BetaFeatures"] = false
            }
        };

        var mutations = new IMutation<FeatureFlagsState>[]
        {
            new EnableFeatureMutation(
                "NewCheckout",
                MutationContext.User("alice", "Alice", "Batch enable") 
                    with { CorrelationId = "BatchToggle1" }
            ),

            new DisableFeatureMutation(
                "LegacyCheckout",
                MutationContext.User("bob", "Bob", "Batch disable") with
                {
                    CorrelationId = "BatchToggle2",
                    Metadata = new Dictionary<string, object> { ["approvedBy"] = "alice,bob" }
                }
            ),

            new EnableFeatureMutation(
                "DarkMode",
                MutationContext.User("carol", "Carol", "Batch enable") with { CorrelationId = "BatchToggle3" }
            ),

            new EnableFeatureMutation(
                "BetaFeatures",
                MutationContext.User("dave", "Dave", "Batch enable") with { CorrelationId = "BatchToggle4" }
            ),
        };

        var batchResult = await engine.ExecuteBatchAsync(mutations, state);
        MutationResultLogger.LogBatch(batchResult.Results);

        Console.WriteLine($"Success: {batchResult.SuccessCount}, Failed: {batchResult.FailureCount}");
        for (int i = 0; i < batchResult.Results.Count; i++)
        {
            var r = batchResult.Results[i];
            var mutationType = mutations[i].GetType().Name;
            Console.WriteLine($"{mutationType}: {(r.IsSuccess ? "✓ Applied" : "✗ Blocked")}");
            if (!r.IsSuccess)
            {
                foreach (var dec in r.PolicyDecisions)
                    Console.WriteLine($"  Policy: {dec.PolicyName} – {dec.Reason}");
            }
        }

        // Poprawka: używamy FinalState z batchResult
        state = batchResult.FinalState ?? state;

        Console.WriteLine($"Final state: {string.Join(", ", state.Flags.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }
}
