using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.WorkflowApprovals.Mutations;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Policies;

/// <summary>
/// Policy that enforces sequential approval of workflow steps in an <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed class EnforceOrderPolicy : IMutationPolicy<ApprovalWorkflowState>
{
    public string Name => "EnforceOrderPolicy";
    public int Priority => 100;
    public string Description => "Ensures steps are approved in order.";
    
    /// <summary>
    /// Evaluates mutation against the policy.
    /// Step can only be approved if the previous step is already approved.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">Current workflow state.</param>
    /// <returns>A <see cref="PolicyDecision"/> indicating whether the mutation is allowed or denied.</returns>
    public PolicyDecision Evaluate(IMutation<ApprovalWorkflowState> mutation, ApprovalWorkflowState state)
    {
        if (mutation is not ApproveStepMutation { StepIndex: > 0 } stepMut) 
            return PolicyDecision.Allow();
        
        if (stepMut.StepIndex - 1 >= state.Steps.Count)
        {
            return PolicyDecision.Deny(
                $"Step index {stepMut.StepIndex} is out of range for the current workflow.");
        }

        var previous = state.Steps[stepMut.StepIndex - 1];
        if (previous.Status != StepStatus.Approved)
        {
            return PolicyDecision.Deny(
                $"Cannot approve step {stepMut.StepIndex} before previous step is approved");
        }
        return PolicyDecision.Allow();
    }
}