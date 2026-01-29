namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when concurrency conflict is detected during mutation execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ConcurrencyException"/> occurs when two or more mutations attempt to modify the
/// same state simultaneously, violating optimistic concurrency rules or integrity constraints.
/// </para>
/// <para>
/// This exception can provide the <see cref="ConflictingExecutionId"/>, which identifies the
/// execution that caused the conflict, useful for diagnostics or retry mechanisms.
/// </para>
/// </remarks>
public sealed class ConcurrencyException : MutationException
{
    /// <summary>
    /// The execution ID of the mutation that caused the conflict, if known.
    /// </summary>
    public string? ConflictingExecutionId { get; init; }

    /// <summary>
    /// Initializes a new <see cref="ConcurrencyException"/> with a specified message.
    /// </summary>
    /// <param name="message">The error message describing the conflict.</param>
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConcurrencyException"/> with a specified message and conflicting execution ID.
    /// </summary>
    /// <param name="message">The error message describing the conflict.</param>
    /// <param name="conflictingExecutionId">The execution ID that caused the conflict.</param>
    public ConcurrencyException(string message, string conflictingExecutionId)
        : base(message)
    {
        ConflictingExecutionId = conflictingExecutionId;
    }
}
