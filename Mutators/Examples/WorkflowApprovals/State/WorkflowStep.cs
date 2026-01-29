namespace Mutators.Examples.WorkflowApprovals.State;

/// <summary>
/// Represents single step in an <see cref="ApprovalWorkflowState"/> workflow.
/// </summary>
public sealed record WorkflowStep(string Name)
{
    /// <summary>
    /// Current status of the step (Pending, Approved, or Rejected).
    /// </summary>
    public StepStatus Status { get; init; } = StepStatus.Pending;

    /// <summary>
    /// User who approved the step, if approved.
    /// </summary>
    public string? ApprovedBy { get; init; }

    /// <summary>
    /// User who rejected the step, if rejected.
    /// </summary>
    public string? RejectedBy { get; init; }
}
