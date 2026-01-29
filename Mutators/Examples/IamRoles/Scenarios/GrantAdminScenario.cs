using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using Mutators.Examples.IamRoles.Mutations;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Scenarios;

/// <summary>
/// GrantAdminScenario
/// 
/// Demonstrates granting the "Admin" role to a single user using <see cref="IMutationEngine"/> 
/// and <see cref="GrantUserRoleMutation"/>.
/// 
/// Scenario Details:
/// - Executes a single <see cref="GrantUserRoleMutation"/> via <see cref="IMutationEngine.ExecuteAsync"/>.
/// - Uses a <see cref="MutationContext"/> for auditing, including the approving user.
/// - Logs whether the mutation succeeded or was blocked by policy.
/// - Handles policy decisions that might prevent the role assignment.
/// - Works with <see cref="UserPermissionsState"/>, tracking roles assigned to users.
/// 
/// Key Steps:
/// 1. Initialize <see cref="UserPermissionsState"/> with the user's current roles.
/// 2. Construct a <see cref="GrantUserRoleMutation"/> to assign the "Admin" role.
/// 3. Execute the mutation with <see cref="IMutationEngine.ExecuteAsync"/>.
/// 4. Log success/failure and any policy decisions that blocked the mutation.
/// 
/// Example Use Case:
/// - Promoting a user to an administrative role in a controlled and auditable manner.
/// - Ensuring that policy evaluation is respected before granting elevated permissions.
/// </summary>
internal static class GrantAdminScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Grant Admin Scenario ===");

        var state = new UserPermissionsState
        {
            RolesByUser = new Dictionary<string, HashSet<string>>
            {
                ["john"] = ["User"]
            }
        };

        var ctx = MutationContext.User(
                userId: "alice",
                userName: "Alice",
                reason: "Promote to admin")
            with
            {
                Metadata = new Dictionary<string, object>
                {
                    ["approvedBy"] = "bob"
                }
            };

        var mutation = new GrantUserRoleMutation("john", "Admin", ctx);

        var result = await engine.ExecuteAsync(mutation, state);

        Console.WriteLine(result.IsSuccess
            ? "✓ Admin role granted"
            : "✗ Grant blocked");

        if (!result.IsSuccess)
        {
            foreach (var decision in result.PolicyDecisions)
                Console.WriteLine($"Policy: {decision.PolicyName} – {decision.Reason}");
        }
    }
}