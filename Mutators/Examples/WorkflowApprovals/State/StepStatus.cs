namespace Mutators.Examples.WorkflowApprovals.State;

/// <summary>
/// Represents the current status of a workflow step in <see cref="ApprovalWorkflowState"/>.
/// </summary>
public enum StepStatus
{
    /// <summary>
    /// Step has not yet been approved or rejected.
    /// </summary>
    Pending,

    /// <summary>
    /// Step has been approved.
    /// </summary>
    Approved,

    /// <summary>
    /// Step has been rejected.
    /// </summary>
    Rejected
}