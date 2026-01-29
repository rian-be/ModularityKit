using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.FeatureFlags.Mutations;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Policies;

/// <summary>
/// Policy that requires two man approval for disabling critical feature flags.
/// </summary>
public sealed class RequireTwoManApprovalPolicy : IMutationPolicy<FeatureFlagsState>
{
    public string Name => nameof(RequireTwoManApprovalPolicy);
    public int Priority => 100;
    public string Description => "Description";
    
    private readonly HashSet<string> _criticalFlags = ["NewCheckout", "BetaFeatures"];

    public PolicyDecision Evaluate(IMutation<FeatureFlagsState> mutation, FeatureFlagsState state)
    {
        if (mutation is not DisableFeatureMutation disable || !_criticalFlags.Contains(disable.FeatureName))
            return PolicyDecision.Allow(Name);

        if (!mutation.Context.Metadata.TryGetValue("approvedBy", out var approvedObj))
            return PolicyDecision.Deny("Critical feature requires two-man approval (none provided)", Name);

        var approvedBy = (approvedObj switch
        {
            string s => s.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)),
            IEnumerable<string> arr => arr,
            _ => (IEnumerable<string>)[]
        })
        .Where(x => !string.Equals(x, mutation.Context.ActorId, StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        return approvedBy.Length switch
        {
            < 2 => PolicyDecision.Deny("Critical feature requires at least two distinct approvers (excluding the actor)", Name),
            _ => PolicyDecision.Allow(Name)
        };
    }
}