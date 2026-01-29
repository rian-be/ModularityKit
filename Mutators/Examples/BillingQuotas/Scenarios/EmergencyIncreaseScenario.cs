using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using Mutators.Examples.BillingQuotas.Mutations;
using Mutators.Examples.BillingQuotas.Policies;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Scenarios;

/// <summary>
/// EmergencyIncreaseScenario
/// 
/// Demonstrates an emergency increase of user quotas, simulating high priority situation where system administrators
/// need to temporarily raise quotas beyond normal limits.
/// 
/// This scenario exercises following capabilities of Mutators framework:
/// 
/// - Batch execution of multiple <see cref="IMutation{TState}"/> instances via <see cref="IMutationEngine.ExecuteBatchAsync"/>
/// - User level <see cref="MutationContext"/> usage for audit and traceability
/// - Policy evaluation and enforcement for high-risk operations
/// - Reporting of successes and policy-denied mutations
/// 
/// Key Steps:
/// 1. Initialize <see cref="QuotaState"/> with example user quotas.
/// 2. Create <see cref="MutationContext"/> representing a system administrator initiating emergency action.
/// 3. Generate <see cref="IncreaseQuotaMutation"/> instances for each affected user.
/// 4. Execute all mutations in batch using <see cref="IMutationEngine.ExecuteBatchAsync"/>.
/// 5. Iterate over the batch results, printing success messages or policy denied reasons.
///
/// Example Use Case:
/// - Emergency quota increase in SaaS billing or subscription systems.
/// - Temporary overrides for high priority customers or system critical operations.
/// 
/// Notes:
/// - Mutations blocked by policies are reported individually but do not prevent execution of other mutations in batch.
/// - This scenario assumes that policies such as <see cref="MaxQuotaPolicy"/> or <see cref="PreventNegativeQuotaPolicy"/>
///   may apply, and demonstrates how violations are surfaced to the operator.
/// </summary>
internal static class EmergencyIncreaseScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Emergency Increase Scenario ===");

        var state = new QuotaState
        {
            UserQuotas = new Dictionary<string, int>
            {
                ["alice"] = 50,
                ["bob"] = 95
            }
        };

        var ctx = MutationContext.User(
            userId: "admin",
            userName: "System Admin",
            reason: "Emergency quota increase");

        var mutations = new IMutation<QuotaState>[]
        {
            new IncreaseQuotaMutation("alice", 15, ctx),
            new IncreaseQuotaMutation("bob", 10, ctx)
        };

        var result = await engine.ExecuteBatchAsync(mutations, state);

        foreach (var res in result.Results)
        {
            if (res.IsSuccess)
                Console.WriteLine($"✓ {res.NewState!.UserQuotas.Keys.First()} quota updated");
            else
            {
                Console.WriteLine("✗ Mutation blocked:");
                foreach (var decision in res.PolicyDecisions)
                    Console.WriteLine($"  Policy: {decision.PolicyName} – {decision.Reason}");
            }
        }
    }
}