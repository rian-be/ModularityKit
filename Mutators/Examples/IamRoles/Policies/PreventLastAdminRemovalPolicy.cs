using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.IamRoles.Mutations;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Policies;

/// <summary>
/// Policy that prevents the removal of last user with "Admin" role.
/// </summary>
internal sealed class PreventLastAdminRemovalPolicy
    : IMutationPolicy<UserPermissionsState>
{
    public string Name => "PreventLastAdminRemoval";
    public int Priority => 100;
    public string Description => "Prevents removal of the last Admin user.";
    
    /// <summary>
    /// Evaluates mutation against the policy.
    /// Only RevokeUserRoleMutation for the "Admin" role is subject to this policy.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current user permissions state.</param>
    /// <returns>A <see cref="PolicyDecision"/> indicating whether the mutation is allowed or denied.</returns>
    public PolicyDecision Evaluate(
        IMutation<UserPermissionsState> mutation,
        UserPermissionsState state)
    {
        if (mutation is not RevokeUserRoleMutation { Role: "Admin" })
            return PolicyDecision.Allow();

        var adminCount = state.RolesByUser
            .SelectMany(kv => kv.Value)
            .Count(r => r == "Admin");

        if (adminCount <= 1)
        {
            return PolicyDecision.Deny(
                "Cannot remove the last Admin user");
        }

        return PolicyDecision.Allow();
    }
}
