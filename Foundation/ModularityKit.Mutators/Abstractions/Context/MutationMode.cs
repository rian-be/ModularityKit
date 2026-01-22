namespace ModularityKit.Mutators.Abstractions.Context;

/// <summary>
/// Defines the execution mode of a mutation.
/// Specifies how the mutation should be processed: simulated, validated, or committed.
/// </summary>
public enum MutationMode
{
    /// <summary>
    /// Simulation mode.
    /// Executes a dry-run of the mutation without applying any actual changes.
    /// Useful for testing, previewing effects, or estimating blast radius.
    /// </summary>
    Simulate = 0,

    /// <summary>
    /// Validation mode.
    /// Checks whether the mutation is valid and allowed, but does not apply it.
    /// Useful for pre-flight checks or policy enforcement.
    /// </summary>
    Validate = 1,

    /// <summary>
    /// Commit mode.
    /// Applies the mutation to the target state, performing the actual change.
    /// Use for production execution.
    /// </summary>
    Commit = 2
}
