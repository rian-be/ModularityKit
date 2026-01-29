namespace ModularityKit.Mutators.Abstractions.Policies;

/// <summary>
/// Registry for mutation policies.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IPolicyRegistry"/> allows registering, unregistering, and retrieving mutation policies
/// for specific state types. Policies are used by the mutation engine to enforce business rules,
/// validations, and authorization logic before applying a mutation.
/// </para>
/// </remarks>
internal interface IPolicyRegistry
{
    /// <summary>
    /// Registers a policy for a given state type.
    /// </summary>
    /// <typeparam name="TState">The type of state the policy applies to.</typeparam>
    /// <param name="policy">The mutation policy to register.</param>
    void Register<TState>(IMutationPolicy<TState> policy);

    /// <summary>
    /// Unregisters a policy by name for a given state type.
    /// </summary>
    /// <typeparam name="TState">The type of state the policy applies to.</typeparam>
    /// <param name="policyName">The name of the policy to remove.</param>
    void Unregister<TState>(string policyName);

    /// <summary>
    /// Retrieves all policies for a given state type.
    /// </summary>
    /// <typeparam name="TState">The type of state.</typeparam>
    /// <returns>An enumerable of policies for the state type.</returns>
    IEnumerable<IMutationPolicy<TState>> GetPolicies<TState>();

    /// <summary>
    /// Retrieves a specific policy by name for a given state type.
    /// </summary>
    /// <typeparam name="TState">The type of state.</typeparam>
    /// <param name="name">The name of the policy.</param>
    /// <returns>The policy if found; otherwise, null.</returns>
    IMutationPolicy<TState>? GetPolicy<TState>(string name);
}
