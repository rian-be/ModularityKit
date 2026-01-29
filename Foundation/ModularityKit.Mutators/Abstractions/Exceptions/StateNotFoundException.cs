namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a mutation cannot find the target state.
/// </summary>
/// <remarks>
/// <see cref="StateNotFoundException"/> indicates that the state identified by <see cref="StateId"/>
/// does not exist in the system, and the mutation cannot proceed.
/// </remarks>
/// <param name="stateId">The identifier of the state that was not found.</param>
public sealed class StateNotFoundException(string stateId) : MutationException($"State with ID '{stateId}' not found")
{
    /// <summary>
    /// Identifier of the state that could not be found.
    /// </summary>
    public string StateId { get; } = stateId;
}
