using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Mutations;

/// <summary>
/// Mutation that disables a feature flag in the current <see cref="FeatureFlagsState"/>.
/// </summary>
internal sealed record DisableFeatureMutation(string FeatureName, MutationContext Context) : IMutation<FeatureFlagsState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "DisableFeature",
        Category = "Configuration",
        RiskLevel = MutationRiskLevel.High,
        Description = "Disables a feature flag."
    };

    public MutationResult<FeatureFlagsState> Apply(FeatureFlagsState state)
    {
        var newFlags = new Dictionary<string, bool>(state.Flags);
        if (newFlags.ContainsKey(FeatureName))
            newFlags[FeatureName] = false;
        
        var newState = state with { Flags = newFlags };
        var changes = ChangeSet.Single(StateChange.Modified($"Flags.{FeatureName}", true, false));
        return MutationResult<FeatureFlagsState>.Success(newState, changes);
    }

    public ValidationResult Validate(FeatureFlagsState state)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(FeatureName))
            result.AddError("FeatureName", "Feature name cannot be empty");
        if (!state.Flags.ContainsKey(FeatureName))
            result.AddError("FeatureName", $"Feature '{FeatureName}' does not exist");
        return result;
    }

    public MutationResult<FeatureFlagsState> Simulate(FeatureFlagsState state) => Apply(state);
}