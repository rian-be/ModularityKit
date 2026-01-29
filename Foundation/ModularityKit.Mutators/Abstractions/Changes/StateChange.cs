namespace ModularityKit.Mutators.Abstractions.Changes;

/// <summary>
/// Represents a single change in the state.
/// Tracks old and new values, type, timestamp, priority, and optional metadata.
/// </summary>
public sealed class StateChange
{
    /// <summary>
    /// Path to the changed value (e.g., "Email", "Address.City").
    /// </summary>
    public string Path { get; private init; } = string.Empty;

    /// <summary>
    /// Value before the change.
    /// </summary>
    public object? OldValue { get; private init; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    public object? NewValue { get; private init; }

    /// <summary>
    /// Type of the change.
    /// </summary>
    internal ChangeType Type { get; init; }

    /// <summary>
    /// Timestamp when the change occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Priority of the change (used for conflict resolution).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Optional additional metadata related to the change.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a <see cref="StateChange"/> representing a modified value.
    /// </summary>
    /// <param name="path">Path of the modified value.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>A <see cref="StateChange"/> instance.</returns>
    public static StateChange Modified(string path, object? oldValue, object? newValue)
        => new()
        {
            Path = path,
            OldValue = oldValue,
            NewValue = newValue,
            Type = ChangeType.Modified
        };

    /// <summary>
    /// Creates a <see cref="StateChange"/> representing an added value.
    /// </summary>
    /// <param name="path">Path of the added value.</param>
    /// <param name="value">Newly added value.</param>
    /// <returns>A <see cref="StateChange"/> instance.</returns>
    public static StateChange Added(string path, object? value)
        => new()
        {
            Path = path,
            NewValue = value,
            Type = ChangeType.Added
        };

    /// <summary>
    /// Creates a <see cref="StateChange"/> representing a removed value.
    /// </summary>
    /// <param name="path">Path of the removed value.</param>
    /// <param name="oldValue">Value before removal.</param>
    /// <returns>A <see cref="StateChange"/> instance.</returns>
    public static StateChange Removed(string path, object? oldValue)
        => new()
        {
            Path = path,
            OldValue = oldValue,
            Type = ChangeType.Removed
        };

    /// <summary>
    /// Creates a <see cref="StateChange"/> representing a replacement (old value replaced by new value).
    /// </summary>
    /// <param name="path">Path of the replaced value.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>A <see cref="StateChange"/> instance.</returns>
    public static StateChange Replaced(string path, object? oldValue, object? newValue)
        => new()
        {
            Path = path,
            OldValue = oldValue,
            NewValue = newValue,
            Type = ChangeType.Replaced
        };
}
