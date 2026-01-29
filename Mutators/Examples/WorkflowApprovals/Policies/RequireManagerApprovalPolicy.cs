using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.WorkflowApprovals.Mutations;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Policies;

/// <summary>
/// Policy that requires workflow steps to be approved by designated manager in <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed class RequireManagerApprovalPolicy : IMutationPolicy<ApprovalWorkflowState>
{
    private readonly HashSet<string> _managers = ["alice", "bob"];

    public string Name => "RequireManagerApproval";
    public int Priority => 100;
    public string Description => "Ensures only managers can approve workflow steps.";
  
    /// <summary>
    /// Evaluates a mutation against the policy.
    /// Only designated managers in <see cref="_managers"/> can approve steps.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">Current workflow state.</param>
    /// <returns>A <see cref="PolicyDecision"/> indicating whether the mutation is allowed or denied.</returns>
    public PolicyDecision Evaluate(IMutation<ApprovalWorkflowState> mutation, ApprovalWorkflowState state)
    {
        if (mutation is not ApproveStepMutation stepMut) return PolicyDecision.Allow();
        if (!_managers.Contains(stepMut.Approver))
        {
            return PolicyDecision.Deny($"Approver '{stepMut.Approver}' is not manager");
        }
        return PolicyDecision.Allow();
    }
}