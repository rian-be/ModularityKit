namespace ModularityKit.Mutators.Abstractions.Results;

/// <summary>
/// Represents a validation error encountered during mutation or state validation.
/// Contains information about the invalid field, the error message, and optional metadata.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Path to the property or field that caused the validation error.
    /// Examples: "Email", "Address.City", "Wallet.Owners[0].Role".
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Human-readable description of the error.
    /// Should clearly explain why the value is invalid.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Optional error code, useful for categorization or localization (i18n).
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// The value that triggered the validation error, if applicable.
    /// Useful for diagnostics and debugging.
    /// </summary>
    public object? AttemptedValue { get; init; }

    /// <summary>
    /// Severity level of the validation error.
    /// Default is <see cref="ValidationSeverity.Error"/>.
    /// </summary>
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;

    /// <summary>
    /// Constructs a new <see cref="ValidationError"/> instance.
    /// </summary>
    /// <param name="path">Path to the invalid property or field.</param>
    /// <param name="message">Description of the validation error.</param>
    /// <param name="code">Optional error code for categorization or localization.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> or <paramref name="message"/> is null.</exception>
    public ValidationError(string path, string message, string? code = null)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = code;
    }

    /// <summary>
    /// Returns a string representation of the validation error,
    /// including severity, path, and message.
    /// </summary>
    /// <returns>A human-readable string describing the validation error.</returns>
    public override string ToString() => $"[{Severity}] {Path}: {Message}";
}
