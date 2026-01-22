namespace ModularityKit.Mutators.Abstractions.Policies;

/// <summary>
/// Represents the severity level of a policy decision regarding a mutation.
/// </summary>
/// <remarks>
/// <para>
/// PolicyDecisionSeverity indicates the criticality or impact of a policy's decision.
/// It can be used for logging, alerting, auditing, and decision prioritization.
/// </para>
/// <list type="bullet">
/// <item><see cref="Info"/> - Informational, no immediate action required.</item>
/// <item><see cref="Warning"/> - Non-critical issue, may require attention.</item>
/// <item><see cref="Error"/> - Action blocked or failed; requires handling.</item>
/// <item><see cref="Critical"/> - High severity, system-critical decision.</item>
/// </list>
/// </remarks>
public enum PolicyDecisionSeverity
{
    /// <summary>
    /// Informational severity; the decision is noted but no action is needed.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning severity; attention may be required but not blocking.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error severity; the mutation is blocked or has failed.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical severity; high-impact decision, requires immediate attention.
    /// </summary>
    Critical = 3
}
