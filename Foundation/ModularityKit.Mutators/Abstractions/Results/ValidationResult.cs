namespace ModularityKit.Mutators.Abstractions.Results;

/// <summary>
/// Represents the result of validating a mutation or state change.
/// Contains errors, warnings, and informational messages produced during validation.
/// </summary>
public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();
    private readonly List<ValidationWarning> _warnings = new();
    private readonly List<ValidationInfo> _info = new();

    /// <summary>
    /// Indicates whether the validation passed successfully.
    /// True if there are no errors; false otherwise.
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Read-only list of validation errors.
    /// Each error indicates a violation of rules that prevents the mutation from being applied.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors;

    /// <summary>
    /// Read-only list of validation warnings.
    /// Warnings indicate potential issues that do not block mutation execution.
    /// </summary>
    public IReadOnlyList<ValidationWarning> Warnings => _warnings;

    /// <summary>
    /// Read-only list of informational messages produced during validation.
    /// </summary>
    public IReadOnlyList<ValidationInfo> Info => _info;

    /// <summary>
    /// Adds a new validation error.
    /// </summary>
    /// <param name="path">The path or property associated with the error.</param>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="code">Optional error code for categorization.</param>
    public void AddError(string path, string message, string? code = null)
        => _errors.Add(new ValidationError(path, message, code));

    /// <summary>
    /// Adds a new validation warning.
    /// </summary>
    /// <param name="path">The path or property associated with the warning.</param>
    /// <param name="message">Human-readable description of the warning.</param>
    /// <param name="code">Optional warning code for categorization.</param>
    public void AddWarning(string path, string message, string? code = null)
        => _warnings.Add(new ValidationWarning(path, message, code));

    /// <summary>
    /// Adds an informational validation message.
    /// </summary>
    /// <param name="path">The path or property associated with the info.</param>
    /// <param name="message">Human-readable informational message.</param>
    public void AddInfo(string path, string message)
        => _info.Add(new ValidationInfo(path, message));

    /// <summary>
    /// Adds an existing <see cref="ValidationError"/> instance.
    /// </summary>
    /// <param name="error">The error to add.</param>
    public void AddError(ValidationError error) => _errors.Add(error);

    /// <summary>
    /// Adds an existing <see cref="ValidationWarning"/> instance.
    /// </summary>
    /// <param name="warning">The warning to add.</param>
    public void AddWarning(ValidationWarning warning) => _warnings.Add(warning);

    /// <summary>
    /// Adds an existing <see cref="ValidationInfo"/> instance.
    /// </summary>
    /// <param name="info">The informational message to add.</param>
    public void AddInfo(ValidationInfo info) => _info.Add(info);

    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    /// <returns>A <see cref="ValidationResult"/> indicating success.</returns>
    public static ValidationResult Success() => new();

    /// <summary>
    /// Creates a validation result with a single error.
    /// </summary>
    /// <param name="path">The path or property associated with the error.</param>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="code">Optional error code.</param>
    /// <returns>A <see cref="ValidationResult"/> containing one error.</returns>
    public static ValidationResult WithError(string path, string message, string? code = null)
    {
        var result = new ValidationResult();
        result.AddError(path, message, code);
        return result;
    }

    /// <summary>
    /// Creates a validation result containing multiple errors.
    /// </summary>
    /// <param name="errors">Array of <see cref="ValidationError"/> to include.</param>
    /// <returns>A <see cref="ValidationResult"/> containing the specified errors.</returns>
    public static ValidationResult WithErrors(params ValidationError[] errors)
    {
        var result = new ValidationResult();
        foreach (var error in errors)
            result.AddError(error);
        return result;
    }
}
