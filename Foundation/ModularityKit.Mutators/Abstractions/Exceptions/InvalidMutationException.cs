namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a mutation is invalidly constructed.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="InvalidMutationException"/> is thrown when a mutation violates construction rules,
/// such as missing required properties, invalid data types, or broken mutator contracts.
/// </para>
/// <para>
/// It can occur during mutation creation or pre-execution validation.
/// </para>
/// </remarks>
/// <param name="message">A descriptive message explaining why the mutation is invalid.</param>
/// <param name="innerException">Optional inner exception that caused this exception.</param>
public sealed class InvalidMutationException(
    string message,
    Exception? innerException = null
) : MutationException(message, innerException ?? new Exception(message))
{
}
