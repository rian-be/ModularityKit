namespace ModularityKit.Mutators.Abstractions.Effects;

/// <summary>
/// Represents a side effect produced by a mutation.
/// Side effects capture additional consequences that are not part of the primary state change.
/// </summary>
public sealed class SideEffect
{
    /// <summary>
    /// The type of the side effect (e.g., "Notification", "AuditLog", "ExternalCall").
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the side effect.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Severity of the side effect.
    /// Determines the criticality or importance for monitoring and alerting.
    /// </summary>
    public SideEffectSeverity Severity { get; init; } = SideEffectSeverity.Info;

    /// <summary>
    /// Optional data associated with the side effect.
    /// Can hold structured information for logging, auditing, or downstream processing.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Timestamp when the side effect occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Indicates whether this side effect requires an explicit action or intervention.
    /// </summary>
    public bool RequiresAction { get; init; }

    /// <summary>
    /// Creates a new <see cref="SideEffect"/> with the specified properties.
    /// </summary>
    /// <param name="type">The type of the side effect.</param>
    /// <param name="description">Human-readable description.</param>
    /// <param name="data">Optional associated data.</param>
    /// <param name="severity">Severity level.</param>
    public static SideEffect Create(
        string type,
        string description,
        object? data = null,
        SideEffectSeverity severity = SideEffectSeverity.Info)
        => new()
        {
            Type = type,
            Description = description,
            Data = data,
            Severity = severity
        };

    /// <summary>
    /// Creates a new critical <see cref="SideEffect"/> instance.
    /// </summary>
    /// <param name="type">The type of the side effect.</param>
    /// <param name="description">Human-readable description.</param>
    /// <param name="data">Optional associated data.</param>
    public static SideEffect Critical(string type, string description, object? data = null)
        => Create(type, description, data, SideEffectSeverity.Critical);
}
