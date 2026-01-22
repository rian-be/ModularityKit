namespace ModularityKit.Mutators.Abstractions.Intent;

/// <summary>
/// Represents the estimated impact scope of a mutation.
/// Used to describe the blast radius—i.e., the potential extent and severity of changes
/// a mutation can have on the system, modules, or entities.
/// </summary>
public sealed class BlastRadius
{
    /// <summary>
    /// The scope of the blast radius.
    /// Determines whether the mutation affects a single element, a module, the system, or globally.
    /// </summary>
    public BlastRadiusScope Scope { get; init; }

    /// <summary>
    /// Estimated number of elements or entities that could be affected by this mutation.
    /// Useful for risk assessment and auditing.
    /// </summary>
    public int? EstimatedAffectedCount { get; init; }

    /// <summary>
    /// A human-readable description of the expected impact of this mutation.
    /// Can include contextual details or limitations of the blast radius.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Creates a blast radius with a single-element scope.
    /// </summary>
    public static BlastRadius Single => new() { Scope = BlastRadiusScope.Single };

    /// <summary>
    /// Creates a blast radius affecting an entire module.
    /// </summary>
    public static BlastRadius Module => new() { Scope = BlastRadiusScope.Module };

    /// <summary>
    /// Creates a blast radius affecting the entire system.
    /// </summary>
    public static BlastRadius System => new() { Scope = BlastRadiusScope.System };

    /// <summary>
    /// Creates a blast radius affecting multiple systems or globally.
    /// </summary>
    public static BlastRadius Global => new() { Scope = BlastRadiusScope.Global };
}
