using ModularityKit.Mutators.Abstractions.Context;

namespace ModularityKit.Mutators.Abstractions.Policies;

/// <summary>
/// Context used during the evaluation of a mutation by policies.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PolicyEvaluationContext"/> encapsulates all the information a policy might need
/// to evaluate whether a mutation is allowed, denied, or requires modifications/approvals.
/// </para>
/// <para>
/// Key points:
/// <list type="bullet">
/// <item><see cref="Mutation"/> is the mutation object being evaluated.</item>
/// <item><see cref="State"/> represents the current state that will be mutated.</item>
/// <item><see cref="MutationContext"/> carries runtime metadata about the mutation execution.</item>
/// <item><see cref="PreviousDecisions"/> allows policies to be aware of earlier policy decisions.</item>
/// <item><see cref="Data"/> is a flexible dictionary for additional context or intermediate results.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class PolicyEvaluationContext
{
    /// <summary>
    /// The mutation being evaluated by policies.
    /// </summary>
    public object Mutation { get; init; } = null!;

    /// <summary>
    /// The current state that will be mutated.
    /// </summary>
    public object State { get; init; } = null!;

    /// <summary>
    /// Runtime metadata and context about the mutation operation.
    /// </summary>
    public MutationContext MutationContext { get; init; } = null!;

    /// <summary>
    /// List of previous decisions made by policies during this evaluation.
    /// </summary>
    public IReadOnlyList<PolicyDecision> PreviousDecisions { get; init; } = [];

    /// <summary>
    /// Flexible dictionary for additional contextual data or intermediate results.
    /// </summary>
    public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
}
