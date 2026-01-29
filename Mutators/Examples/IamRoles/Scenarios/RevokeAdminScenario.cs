using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using Mutators.Examples.IamRoles.Mutations;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Scenarios;

/// <summary>
/// RevokeAdminScenario
/// 
/// Demonstrates revoking the "Admin" role from a user using <see cref="IMutationEngine"/> 
/// and <see cref="RevokeUserRoleMutation"/>.
/// 
/// Scenario Details:
/// - Executes a single <see cref="RevokeUserRoleMutation"/> via <see cref="IMutationEngine.ExecuteAsync"/>.
/// - Uses a <see cref="MutationContext.System"/> context for system-initiated auditing.
/// - Logs whether the revocation succeeded or was blocked by policy.
/// - Handles policy decisions that might prevent revoking the role.
/// - Works with <see cref="UserPermissionsState"/>, tracking roles assigned to users.
/// 
/// Key Steps:
/// 1. Initialize <see cref="UserPermissionsState"/> with the user's current roles.
/// 2. Construct a <see cref="RevokeUserRoleMutation"/> to remove the "Admin" role.
/// 3. Execute the mutation with <see cref="IMutationEngine.ExecuteAsync"/>.
/// 4. Log success/failure and any policy decisions that blocked the revocation.
/// 
/// Example Use Case:
/// - Removing administrative privileges for security or compliance purposes.
/// - Ensuring that policy evaluation is respected before revoking elevated permissions.
/// </summary>
internal static class RevokeAdminScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Revoke Admin Scenario ===");

        var state = new UserPermissionsState
        {
            RolesByUser = new Dictionary<string, HashSet<string>>
            {
                ["john"] = ["Admin"]
            }
        };

        var ctx = MutationContext.System(
            reason: "Security cleanup");

        var mutation = new RevokeUserRoleMutation("john", "Admin", ctx);

        var result = await engine.ExecuteAsync(mutation, state);

        Console.WriteLine(result.IsSuccess
            ? "✓ Admin role revoked"
            : "✗ Revocation blocked");

        if (!result.IsSuccess)
        {
            foreach (var decision in result.PolicyDecisions)
                Console.WriteLine($"Policy: {decision.PolicyName} – {decision.Reason}");
        }
    }
}