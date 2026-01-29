using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Policies;

/// <summary>
/// Policy that requires second user approval for critical mutations in <see cref="UserPermissionsState"/>.
/// </summary>
public sealed class RequireTwoManApprovalPolicy
    : IMutationPolicy<UserPermissionsState>
{
    public string Name => "RequireTwoManApproval";
    public int Priority => 100;
    public string Description => "Requires second user approval for critical mutations.";
    
    /// <summary>
    /// Evaluates mutation against the policy.
    /// Critical mutations must have a second approver that is different from the actor.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current user permissions state.</param>
    /// <returns>A <see cref="PolicyDecision"/> indicating whether the mutation is allowed or denied.</returns>
    public PolicyDecision Evaluate(
        IMutation<UserPermissionsState> mutation,
        UserPermissionsState state)
    {
        if (mutation.Intent.RiskLevel != MutationRiskLevel.Critical)
            return PolicyDecision.Allow();

        var context = mutation.Context;

        if (!context.Metadata.TryGetValue("approvedBy", out var approvedObj))
        {
            return PolicyDecision.Deny(
                "Critical mutation requires second approval (approvedBy missing)");
        }

        var approvers = approvedObj switch
        {
            string s => s.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)),
            IEnumerable<string> arr => arr,
            _ => [approvedObj?.ToString() ?? string.Empty]
        };

        if (approvers.Any(a => string.Equals(a, context.ActorId, StringComparison.OrdinalIgnoreCase)))
        {
            return PolicyDecision.Deny(
                "Second approval must come from a different user");
        }

        return PolicyDecision.Allow();
    }
}