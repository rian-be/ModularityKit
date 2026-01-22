namespace ModularityKit.Mutators.Abstractions.Results;

// <summary>
/// Specifies the severity level of a validation message.
/// Used in <see cref="ValidationError"/>, <see cref="ValidationWarning"/>, and related validation constructs.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Informational message. Does not indicate a problem.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning. Indicates a potential issue that may need attention.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error. Indicates a validation failure that should be addressed.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical error. Indicates a severe failure requiring immediate attention.
    /// </summary>
    Critical = 3
}
