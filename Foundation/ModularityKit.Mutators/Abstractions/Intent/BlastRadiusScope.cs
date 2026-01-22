namespace ModularityKit.Mutators.Abstractions.Intent;

/// <summary>
/// Defines the scope of impact for a mutation.
/// Determines the extent of the system or components affected by the mutation.
/// </summary>
public enum BlastRadiusScope
{
    /// <summary>
    /// Single element scope.
    /// The mutation affects only a single targeted entity or object.
    /// </summary>
    Single = 0,

    /// <summary>
    /// Module scope.
    /// The mutation impacts an entire module, component, or bounded context.
    /// </summary>
    Module = 1,

    /// <summary>
    /// System scope.
    /// The mutation affects the entire system or service.
    /// </summary>
    System = 2,

    /// <summary>
    /// Global scope.
    /// The mutation has a global effect across multiple systems or services.
    /// </summary>
    Global = 3
}
