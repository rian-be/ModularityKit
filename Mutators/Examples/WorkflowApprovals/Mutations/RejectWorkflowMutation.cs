using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Mutations;

/// <summary>
/// Mutation that rejects the entire workflow in an <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed record RejectWorkflowMutation(
    string Rejector,
    MutationContext Context
) : IMutation<ApprovalWorkflowState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "RejectWorkflow",
        Category = "Workflow",
        RiskLevel = MutationRiskLevel.Critical,
        Description = "Rejects the entire workflow"
    };

    public ValidationResult Validate(ApprovalWorkflowState state)
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(Rejector))
            result.AddError("Reject", "Reject cannot be empty");
        return result;
    }

    public MutationResult<ApprovalWorkflowState> Apply(ApprovalWorkflowState state)
    {
        var steps = state.Steps.Select(s => s with
        {
            Status = StepStatus.Rejected,
            RejectedBy = Rejector
        }).ToList();

        var newState = state with { Steps = steps };
        var changes = ChangeSet.Single(StateChange.Modified("Workflow", null, "Rejected"));
        return MutationResult<ApprovalWorkflowState>.Success(newState, changes);
    }

    public MutationResult<ApprovalWorkflowState> Simulate(ApprovalWorkflowState state) => Apply(state);
}