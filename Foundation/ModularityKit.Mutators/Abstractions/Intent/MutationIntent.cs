namespace ModularityKit.Mutators.Abstractions.Intent;

/// <summary>
/// Represents the intent behind a mutation — what change is being made and why.
/// </summary>
/// <remarks>
/// <para>
/// MutationIntent encapsulates the purpose, context, and metadata of a mutation operation.
/// It is used by the Mutators framework to classify, audit, and control changes across
/// the system.
/// </para>
/// <para>
/// Key considerations:
/// <list type="bullet">
/// <item><see cref="OperationName"/> identifies the action (e.g., "EnableFeature", "GrantPermission").</item>
/// <item><see cref="Category"/> groups mutations into domains such as Security, Configuration, or Domain.</item>
/// <item><see cref="RiskLevel"/> guides review and approval workflows.</item>
/// <item><see cref="IsReversible"/> indicates whether the mutation can be safely undone.</item>
/// <item><see cref="EstimatedBlastRadius"/> estimates the potential impact scope.</item>
/// <item><see cref="Tags"/> and <see cref="Metadata"/> allow rich classification and tracing.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var intent = MutationIntent.Create("EnableFeature", "Configuration");
/// intent.Description = "Enable new payment processing feature for beta users.";
/// intent.RiskLevel = MutationRiskLevel.Medium;
/// intent.IsReversible = true;
/// intent.Tags = new HashSet&lt;string&gt; { "beta", "payment" };
/// intent.Metadata["initiator"] = "admin@ryze.com";
/// </code>
/// </example>
public sealed class MutationIntent
{
    /// <summary>
    /// Name of the operation being performed (e.g., "EnableFeature", "GrantPermission").
    /// </summary>
    public string OperationName { get; init; } = string.Empty;

    /// <summary>
    /// Category of the mutation (e.g., "Security", "Configuration", "Domain").
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Business-level description explaining why this change is being performed.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Risk level associated with this mutation.
    /// </summary>
    public MutationRiskLevel RiskLevel { get; init; } = MutationRiskLevel.Low;

    /// <summary>
    /// Indicates whether the mutation is reversible.
    /// </summary>
    public bool IsReversible { get; init; } = true;

    /// <summary>
    /// Estimated scope of impact (blast radius) of the mutation.
    /// </summary>
    public BlastRadius? EstimatedBlastRadius { get; init; }

    /// <summary>
    /// Classification tags for the mutation.
    /// </summary>
    public IReadOnlySet<string> Tags { get; init; } = new HashSet<string>();

    /// <summary>
    /// Additional metadata associated with the mutation.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }
        = new Dictionary<string, object>();

    /// <summary>
    /// Timestamp of when the mutation intent was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Factory method for creating a <see cref="MutationIntent"/> with required fields.
    /// </summary>
    /// <param name="operationName">The name of the mutation operation.</param>
    /// <param name="category">The category of the mutation.</param>
    /// <returns>A new instance of <see cref="MutationIntent"/>.</returns>
    public static MutationIntent Create(string operationName, string category)
        => new() { OperationName = operationName, Category = category };
}
