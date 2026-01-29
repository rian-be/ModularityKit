using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.FeatureFlags.Mutations;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Scenarios;

/// <summary>
/// DisableLegacyCheckoutScenario
/// 
/// Demonstrates the execution of a single feature flag mutation to disable the "LegacyCheckout" feature
/// using <see cref="IMutationEngine"/> and <see cref="IMutation{TState}"/> abstractions.
///
/// This scenario exercises:
/// 
/// - Execution of a single mutation via <see cref="IMutationEngine.ExecuteAsync"/>
/// - Enforcement of policies via <see cref="PolicyDecision"/>
/// - Use of <see cref="MutationContext"/> for user correlation, auditing, and approvals
/// - Handling of policy decisions that may block the mutation
/// - Updating the state after a successful mutation
/// 
/// Key Steps:
/// 1. Initialize <see cref="FeatureFlagsState"/> with "LegacyCheckout" and "NewCheckout" enabled.
/// 2. Construct a <see cref="DisableFeatureMutation"/> with a <see cref="MutationContext"/> containing
///    correlation ID and approval metadata.
/// 3. Execute the mutation using <see cref="IMutationEngine.ExecuteAsync"/>.
/// 4. Inspect <see cref="MutationResult{TState}.IsSuccess"/> to check if the mutation was applied.
/// 5. Log blocked policy decisions if the mutation was denied.
/// 6. Output the final feature flag state.
///
/// Example Use Case:
/// - Disabling an outdated checkout flow after a new version is live.
/// - Enforcing approvals before disabling critical features.
/// - Auditing feature flag changes with user metadata and correlation IDs.
/// 
/// Notes:
/// - The mutation may be blocked if policies such as "RequireTwoManApprovalPolicy" or
///   "BusinessHoursPolicy" deny it.
/// - Metadata can be extended to include approvers, reasons, or other audit information.
/// </summary>
internal static class DisableLegacyCheckoutScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Disable LegacyCheckout Scenario ===");

        var state = new FeatureFlagsState
        {
            Flags = new Dictionary<string, bool>
            {
                ["LegacyCheckout"] = true,
                ["NewCheckout"] = true
            }
        };
        
        var ctx = MutationContext.User("bob", "Bob", "Disable legacy checkout") with
        {
            CorrelationId = "DisableLegacyCheckout",
            Metadata = new Dictionary<string, object>
            {
                ["approvedBy"] = "alice"
            }
        };

        var mutation = new DisableFeatureMutation("LegacyCheckout", ctx);

        var result = await engine.ExecuteAsync(mutation, state);

        if (result.IsSuccess)
        {
            Console.WriteLine("✓ LegacyCheckout disabled!");
            state = result.NewState!;
        }
        else
        {
            Console.WriteLine("✗ Mutation blocked:");
            foreach (var dec in result.PolicyDecisions)
                Console.WriteLine($"  {dec.PolicyName} – {dec.Reason}");
        }

        Console.WriteLine($"Current state: {string.Join(", ", state.Flags.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }
}