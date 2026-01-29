namespace ModularityKit.Mutators.Abstractions.Policies;

/// <summary>
/// Represents the decision of a policy regarding a mutation.
/// Contains approval, denial, modification instructions, and metadata.
/// </summary>
public sealed class PolicyDecision
{
    /// <summary>
    /// Indicates whether the mutation is allowed.
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Reason for the decision (human-readable).
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Name of the policy that made this decision.
    /// </summary>
    public string? PolicyName { get; init; }

    /// <summary>
    /// Severity of the decision (Info, Warning, Error, Critical).
    /// </summary>
    public PolicyDecisionSeverity Severity { get; init; } = PolicyDecisionSeverity.Info;

    /// <summary>
    /// Modifications to apply if the policy alters the mutation.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Modifications { get; init; }

    /// <summary>
    /// Requirements that must be fulfilled (e.g., approvals) for the mutation.
    /// </summary>
    public IReadOnlyList<PolicyRequirement>? Requirements { get; init; }

    /// <summary>
    /// Additional metadata for diagnostics or logging.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Timestamp when the decision was made.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an allow decision.
    /// </summary>
    /// <param name="policyName">Optional policy name.</param>
    /// <param name="reason">Optional human-readable reason.</param>
    /// <returns>A policy decision that allows the mutation.</returns>
    public static PolicyDecision Allow(string? policyName = null, string? reason = null)
        => new()
        {
            IsAllowed = true,
            PolicyName = policyName,
            Reason = reason
        };

    /// <summary>
    /// Creates a deny decision with standard error severity.
    /// </summary>
    /// <param name="reason">Reason for denial.</param>
    /// <param name="policyName">Optional policy name.</param>
    /// <returns>A policy decision that denies the mutation.</returns>
    public static PolicyDecision Deny(string reason, string? policyName = null)
        => new()
        {
            IsAllowed = false,
            Reason = reason,
            PolicyName = policyName,
            Severity = PolicyDecisionSeverity.Error
        };

    /// <summary>
    /// Creates a deny decision with critical severity.
    /// </summary>
    /// <param name="reason">Reason for denial.</param>
    /// <param name="policyName">Optional policy name.</param>
    /// <returns>A critical denial policy decision.</returns>
    public static PolicyDecision DenyCritical(string reason, string? policyName = null)
        => new()
        {
            IsAllowed = false,
            Reason = reason,
            PolicyName = policyName,
            Severity = PolicyDecisionSeverity.Critical
        };

    /// <summary>
    /// Creates a modification decision that adjusts mutation values.
    /// </summary>
    /// <param name="modifications">Dictionary of modifications to apply.</param>
    /// <param name="policyName">Optional policy name.</param>
    /// <returns>A policy decision that modifies the mutation.</returns>
    public static PolicyDecision Modify(
        IReadOnlyDictionary<string, object> modifications,
        string? policyName = null)
        => new()
        {
            IsAllowed = true,
            Modifications = modifications,
            PolicyName = policyName
        };

    /// <summary>
    /// Creates a decision that requires additional approval before proceeding.
    /// </summary>
    /// <param name="requirement">The requirement that must be fulfilled.</param>
    /// <param name="policyName">Optional policy name.</param>
    /// <returns>A policy decision requiring approval.</returns>
    public static PolicyDecision RequireApproval(
        PolicyRequirement requirement,
        string? policyName = null)
        => new()
        {
            IsAllowed = false,
            Requirements = new[] { requirement },
            PolicyName = policyName,
            Reason = "Requires approval"
        };
}
