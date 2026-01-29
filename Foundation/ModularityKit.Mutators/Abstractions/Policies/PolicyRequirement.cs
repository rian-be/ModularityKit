namespace ModularityKit.Mutators.Abstractions.Policies;

/// <summary>
/// Represents a requirement that must be fulfilled before a mutation can proceed.
/// </summary>
/// <remarks>
/// <para>
/// Policy requirements are used in conjunction with <see cref="PolicyDecision"/> to enforce
/// governance rules such as approvals, additional authentication, or compliance checks.
/// </para>
/// <para>
/// Key points:
/// <list type="bullet">
/// <item><see cref="Type"/> identifies the kind of requirement (e.g., "Approval", "TwoFactor").</item>
/// <item><see cref="Description"/> provides a human-readable explanation of the requirement.</item>
/// <item><see cref="Data"/> can carry structured information relevant to the requirement.</item>
/// <item><see cref="IsFulfilled"/> indicates whether the requirement has already been satisfied.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class PolicyRequirement
{
    /// <summary>
    /// Type of the requirement (e.g., "Approval", "TwoFactor").
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the requirement.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Optional structured data associated with the requirement.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Indicates whether the requirement has been fulfilled.
    /// </summary>
    public bool IsFulfilled { get; init; }

    /// <summary>
    /// Creates an approval requirement specifying who must approve and why.
    /// </summary>
    /// <param name="approver">Identifier of the approver (e.g., email or username).</param>
    /// <param name="reason">Reason for requiring approval.</param>
    /// <returns>A policy requirement representing an approval step.</returns>
    public static PolicyRequirement Approval(string approver, string reason)
        => new()
        {
            Type = "Approval",
            Description = $"Requires approval from {approver}: {reason}",
            Data = new { Approver = approver, Reason = reason }
        };

    /// <summary>
    /// Creates a two-factor authentication requirement.
    /// </summary>
    /// <returns>A policy requirement representing a 2FA step.</returns>
    public static PolicyRequirement TwoFactor()
        => new()
        {
            Type = "TwoFactor",
            Description = "Requires two-factor authentication"
        };
}
