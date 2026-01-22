namespace ModularityKit.Mutators.Abstractions.Results;

/// <summary>
/// Represents an informational message encountered during mutation or state validation.
/// Infos are non-critical and serve purely as guidance or context for developers/operators.
/// </summary>
/// <remarks>
/// Constructs a new <see cref="ValidationInfo"/> instance.
/// </remarks>
/// <param name="path">Path to the property or concept for this informational message.</param>
/// <param name="message">The informational message.</param>
/// <exception cref="ArgumentNullException">
/// Thrown if <paramref name="path"/> or <paramref name="message"/> is null.
/// </exception>
public sealed class ValidationInfo(string path, string message)
{
    /// <summary>
    /// Path to the property, field, or concept related to the information.
    /// Examples: "Wallet.Balance", "Owner.Address.City".
    /// </summary>
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));

    /// <summary>
    /// Human-readable informational message.
    /// Should provide context or explanation without indicating an error or warning.
    /// </summary>
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// Returns a string representation of the info,
    /// including the info tag, path, and message.
    /// </summary>
    /// <returns>A human-readable string describing the informational message.</returns>
    public override string ToString() => $"[INFO] {Path}: {Message}";
}
