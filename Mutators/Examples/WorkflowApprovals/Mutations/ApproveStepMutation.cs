using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Mutations;

/// <summary>
/// Mutation that approves specific step in an <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed record ApproveStepMutation(
    int StepIndex,
    string Approver,
    MutationContext Context
) : IMutation<ApprovalWorkflowState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "ApproveStep",
        Category = "Workflow",
        RiskLevel = MutationRiskLevel.High,
        Description = "Approve a workflow step"
    };

    public ValidationResult Validate(ApprovalWorkflowState state)
    {
        var result = new ValidationResult();
        if (StepIndex < 0 || StepIndex >= state.Steps.Count)
            result.AddError("StepIndex", "Invalid step index");
        if (string.IsNullOrEmpty(Approver))
            result.AddError("Approver", "Approver cannot be empty");
        return result;
    }

    public MutationResult<ApprovalWorkflowState> Apply(ApprovalWorkflowState state)
    {
        var steps = state.Steps.ToList();
        var oldStep = steps[StepIndex];
        var newStep = oldStep with
        {
            Status = StepStatus.Approved,
            ApprovedBy = Approver
        };
        steps[StepIndex] = newStep;

        var newState = state with { Steps = steps };

        var changes = ChangeSet.Single(
            StateChange.Modified($"Steps[{StepIndex}]", oldStep.Status, newStep.Status)
        );

        return MutationResult<ApprovalWorkflowState>.Success(newState, changes);
    }

    public MutationResult<ApprovalWorkflowState> Simulate(ApprovalWorkflowState state) => Apply(state);
}