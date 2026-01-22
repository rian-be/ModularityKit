namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Base exception type for errors occurring during a mutation execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MutationException"/> is thrown whenever a mutation fails due to internal errors,
/// policy violations, or unexpected state inconsistencies. It carries an optional
/// <see cref="ExecutionId"/> to correlate the exception with a specific mutation run.
/// </para>
/// <para>
/// Common usage scenarios include:
/// <list type="bullet">
/// <item>Mutation execution failures inside a <c>Mutator</c>.</item>
/// <item>Interception or auditing errors during mutation lifecycle.</item>
/// <item>Unexpected state validation or integrity violations.</item>
/// </list>
/// </para>
/// </remarks>
public class MutationException : Exception
{
    /// <summary>
    /// Optional unique identifier of the mutation execution that caused the exception.
    /// </summary>
    public string? ExecutionId { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="MutationException"/> with a specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    public MutationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MutationException"/> with a specified message
    /// and an inner exception that caused this error.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="innerException">The underlying exception that triggered this error.</param>
    public MutationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
