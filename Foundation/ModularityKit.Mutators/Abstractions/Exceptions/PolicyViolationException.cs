using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a mutation is blocked by a policy.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PolicyViolationException"/> is raised whenever a mutation is rejected by one or more
/// <see cref="IMutationPolicy{TState}"/> evaluations. This exception provides access to the
/// <see cref="PolicyDecision"/> that caused the blockage, including the reason and severity.
/// </para>
/// <para>
/// Typical usage scenarios:
/// <list type="bullet">
/// <item>Mutations violating security, configuration, or business rules.</item>
/// <item>Mutations requiring approval or additional preconditions.</item>
/// <item>Mutations that exceed risk thresholds or compliance requirements.</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="decision">The policy decision that blocked the mutation.</param>
public sealed class PolicyViolationException(PolicyDecision decision) : MutationException($"Mutation blocked by policy '{decision.PolicyName}': {decision.Reason}")
{
    /// <summary>
    /// The policy decision that caused the mutation to be blocked.
    /// </summary>
    public PolicyDecision Decision { get; } = decision;
}
