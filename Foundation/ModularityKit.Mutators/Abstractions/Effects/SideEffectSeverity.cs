namespace ModularityKit.Mutators.Abstractions.Effects;

/// <summary>
/// Represents the severity level of a <see cref="SideEffect"/>.
/// Determines the criticality and potential need for intervention.
/// </summary>
public enum SideEffectSeverity
{
    /// <summary>
    /// Informational side effect. No immediate action required.
    /// </summary>
    Info = 0,
    
    /// <summary>
    /// Warning-level side effect. Indicates a potential issue, may require attention.
    /// </summary>
    Warning = 1,
    
    /// <summary>
    /// Error-level side effect. Indicates a problem that should be addressed.
    /// </summary>
    Error = 2,
    
    /// <summary>
    /// Critical side effect. Immediate action is recommended. May impact system stability or security.
    /// </summary>
    Critical = 3
}
