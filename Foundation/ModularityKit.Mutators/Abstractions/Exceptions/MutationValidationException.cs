using ModularityKit.Mutators.Abstractions.Results;

namespace ModularityKit.Mutators.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a mutation fails validation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MutationValidationException"/> is raised whenever the mutation framework
/// detects validation errors in the mutation data or the target state before applying changes.
/// It wraps a <see cref="ValidationResult"/> detailing all the validation issues encountered.
/// </para>
/// <para>
/// The <see cref="ValidationResult"/> contains a list of <c>Errors</c> with paths and messages,
/// allowing precise identification of invalid fields or rules that failed.
/// </para>
/// <para>
/// Typical usage scenarios:
/// <list type="bullet">
/// <item>Field-level validation failures (e.g., missing required data, format errors).</item>
/// <item>Business rule violations detected before mutation execution.</item>
/// <item>Preconditions not met for applying certain changes.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class MutationValidationException : MutationException
{
    /// <summary>
    /// Detailed validation result that caused this exception.
    /// </summary>
    public ValidationResult ValidationResult { get; }

    /// <summary>
    /// Initializes a new <see cref="MutationValidationException"/> with a generated message
    /// from the specified <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="validationResult">The validation result containing errors.</param>
    public MutationValidationException(ValidationResult validationResult)
        : base(BuildMessage(validationResult)) => ValidationResult = validationResult;

    /// <summary>
    /// Initializes a new <see cref="MutationValidationException"/> with a custom message
    /// and a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="message">Custom human-readable error message.</param>
    /// <param name="validationResult">The validation result containing errors.</param>
    public MutationValidationException(string message, ValidationResult validationResult)
        : base(message) => ValidationResult = validationResult;

    private static string BuildMessage(ValidationResult result)
    {
        var errors = string.Join(Environment.NewLine + "  - ",
            result.Errors.Select(e => $"{e.Path}: {e.Message}"));

        return $"Mutation validation failed:{Environment.NewLine}  - {errors}";
    }
}
