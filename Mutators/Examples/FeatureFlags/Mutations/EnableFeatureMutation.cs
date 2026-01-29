using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.FeatureFlags.State;

namespace Mutators.Examples.FeatureFlags.Mutations;

/// <summary>
/// Mutation that enables feature flag in the current <see cref="FeatureFlagsState"/>.
/// </summary>
internal sealed record EnableFeatureMutation(string FeatureName, MutationContext Context) : IMutation<FeatureFlagsState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "EnableFeature",
        Category = "Security",
        Tags = new HashSet<string> { "auth" },
        RiskLevel = MutationRiskLevel.High,
        Description = "Enables a feature flag."
    };

    public MutationResult<FeatureFlagsState> Apply(FeatureFlagsState state)
    {
        if (state.Flags.TryGetValue(FeatureName, out var oldValue) && oldValue)
            return MutationResult<FeatureFlagsState>.Success(state, ChangeSet.Empty);

        var newFlags = new Dictionary<string, bool>(state.Flags)
        {
            [FeatureName] = true
        };
        var newState = state with { Flags = newFlags };
        var changes = ChangeSet.Single(
            StateChange.Modified($"Flags.{FeatureName}", oldValue, true)
        );
        return MutationResult<FeatureFlagsState>.Success(newState, changes);
    }

    public ValidationResult Validate(FeatureFlagsState state)
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(FeatureName))
        {
            result.AddError("FeatureName", "Feature name cannot be empty");
        }
        else if (!state.Flags.ContainsKey(FeatureName))
        {
            result.AddError("FeatureName", $"Feature '{FeatureName}' does not exist");
        }
        return result;
    }

    public MutationResult<FeatureFlagsState> Simulate(FeatureFlagsState state) => Apply(state);
}