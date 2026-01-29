namespace ModularityKit.Mutators.Abstractions.Intent;

/// <summary>
/// Specifies the risk level associated with a mutation operation.
/// </summary>
/// <remarks>
/// <para>
/// Mutations in the system can have varying levels of impact on entities or application state.
/// This enum is used to classify the potential risk of a mutation, guiding approval, auditing,
/// and review workflows.
/// </para>
/// <para>
/// Typical usage:
/// <list type="bullet">
/// <item>Low — safe, non-disruptive changes.</item>
/// <item>Medium — requires attention and validation.</item>
/// <item>High — requires additional verification, testing, or review.</item>
/// <item>Critical — requires formal approval and audit before execution.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// MutationRiskLevel risk = MutationRiskLevel.High;
/// if (risk >= MutationRiskLevel.Critical)
/// {
///     ApproveAndAuditMutation();
/// }
/// </code>
/// </example>
public enum MutationRiskLevel
{
    /// <summary>
    /// Low risk — safe, non-disruptive changes.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium risk — requires attention and validation.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High risk — requires additional verification or review.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical risk — requires formal approval and audit before execution.
    /// </summary>
    Critical = 3
}
