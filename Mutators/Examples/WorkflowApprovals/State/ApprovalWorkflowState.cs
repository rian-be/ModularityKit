namespace Mutators.Examples.WorkflowApprovals.State;

/// <summary>
/// Represents the current state of an approval workflow, including steps, workflow ID, and initiator.
/// </summary>
public sealed record ApprovalWorkflowState
{
    /// <summary>
    /// Unique identifier of the workflow.
    /// </summary>
    public string WorkflowId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// List of steps in the workflow.
    /// </summary>
    public IReadOnlyList<WorkflowStep> Steps { get; init; } = new List<WorkflowStep>();

    /// <summary>
    /// User who initiated the workflow.
    /// </summary>
    public string Initiator { get; init; } = "";
}
