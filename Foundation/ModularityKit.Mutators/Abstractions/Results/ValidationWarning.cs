namespace ModularityKit.Mutators.Abstractions.Results;

/// <summary>
/// Represents a validation warning encountered during mutation or state validation.
/// Warnings indicate potential issues that do not block execution but may require attention.
/// </summary>
/// <remarks>
/// Constructs a new <see cref="ValidationWarning"/> instance.
/// </remarks>
/// <param name="path">Path to the property or field causing the warning.</param>
/// <param name="message">Description of the warning.</param>
/// <param name="code">Optional code for categorization or localization.</param>
/// <exception cref="ArgumentNullException">
/// Thrown if <paramref name="path"/> or <paramref name="message"/> is null.
/// </exception>
public sealed class ValidationWarning(string path, string message, string? code = null)
{
    /// <summary>
    /// Path to the property or field that triggered the warning.
    /// Examples: "Email", "Address.City", "Wallet.Owners[0].Role".
    /// </summary>
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));

    /// <summary>
    /// Human-readable description of the warning.
    /// Should clearly explain the potential issue or caution.
    /// </summary>
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// Optional code for categorization, tracking, or localization (i18n).
    /// </summary>
    public string? Code { get; } = code;

    /// <summary>
    /// Returns a string representation of the warning,
    /// including severity tag, path, and message.
    /// </summary>
    /// <returns>A human-readable string describing the warning.</returns>
    public override string ToString() => $"[WARNING] {Path}: {Message}";
}
