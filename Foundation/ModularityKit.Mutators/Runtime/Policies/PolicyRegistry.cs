using ModularityKit.Mutators.Abstractions.Policies;

namespace ModularityKit.Mutators.Runtime.Policies;

/// <summary>
/// Thread-safe registry for mutation policies.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PolicyRegistry"/> manages policies for different state types, allowing
/// registration, unregistration, and retrieval of policies. Policies are stored per state type,
/// and the registry ensures thread-safe access using an internal lock.
/// </para>
/// </remarks>
internal sealed class PolicyRegistry : IPolicyRegistry
{
    private readonly Dictionary<Type, List<object>> _policies = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Registers a policy for a specific state type.
    /// </summary>
    /// <typeparam name="TState">The type of state the policy applies to.</typeparam>
    /// <param name="policy">The policy to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
    public void Register<TState>(IMutationPolicy<TState> policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        lock (_lock)
        {
            var stateType = typeof(TState);

            if (!_policies.ContainsKey(stateType))
                _policies[stateType] = [];

            _policies[stateType].Add(policy);
        }
    }

    /// <summary>
    /// Unregisters a policy by name for a specific state type.
    /// </summary>
    /// <typeparam name="TState">The type of state the policy applies to.</typeparam>
    /// <param name="policyName">The name of the policy to remove.</param>
    public void Unregister<TState>(string policyName)
    {
        lock (_lock)
        {
            var stateType = typeof(TState);

            if (_policies.TryGetValue(stateType, out var policies))
            {
                policies.RemoveAll(p =>
                    p is IMutationPolicy<TState> policy && policy.Name == policyName);
            }
        }
    }

    /// <summary>
    /// Retrieves all policies for a specific state type.
    /// </summary>
    /// <typeparam name="TState">The type of state.</typeparam>
    /// <returns>An enumerable of registered policies for the state type.</returns>
    public IEnumerable<IMutationPolicy<TState>> GetPolicies<TState>()
    {
        lock (_lock)
        {
            var stateType = typeof(TState);

            if (_policies.TryGetValue(stateType, out var policies))
            {
                return policies.Cast<IMutationPolicy<TState>>().ToList();
            }

            return Enumerable.Empty<IMutationPolicy<TState>>();
        }
    }

    /// <summary>
    /// Retrieves a single policy by name for a specific state type.
    /// </summary>
    /// <typeparam name="TState">The type of state.</typeparam>
    /// <param name="name">The name of the policy.</param>
    /// <returns>The policy if found; otherwise, null.</returns>
    public IMutationPolicy<TState>? GetPolicy<TState>(string name)
    {
        return GetPolicies<TState>().FirstOrDefault(p => p.Name == name);
    }
}
