namespace Mutators.Examples.FeatureFlags.State;

/// <summary>
/// Represents the current state of all feature flags.
/// </summary>
public sealed record FeatureFlagsState
{
    /// <summary>
    /// A read-only dictionary of feature flags and their enabled/disabled status.
    /// </summary>
    public IReadOnlyDictionary<string, bool> Flags { get; init; }
        = new Dictionary<string, bool>();
}
