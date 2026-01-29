using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Policies;

/// <summary>
/// Policy that restricts high risk mutations to business hours (9:00-17:00 UTC).
/// </summary>
public class BusinessHoursPolicy : IMutationPolicy<FeatureFlagsState>
{
    public string Name => "BusinessHoursPolicy";
    public int Priority => 100;
    public string Description => "High-risk mutations are only allowed during business hours (9:00-17:00 UTC).";

    public PolicyDecision Evaluate(IMutation<FeatureFlagsState> mutation, FeatureFlagsState state)
    {
        if (mutation.Intent.RiskLevel < MutationRiskLevel.High) return PolicyDecision.Allow(Name);
        var hour = DateTime.UtcNow.Hour;

        if (hour is < 9 or >= 17)
        {
            return PolicyDecision.Deny(
                "High-risk changes are only allowed during business hours (9-17 UTC).",
                Name);
        }
        return PolicyDecision.Allow(Name);
    }
}