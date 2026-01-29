using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using Mutators.Examples.WorkflowApprovals.Mutations;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Scenarios;

/// <summary>
/// RejectedScenario
/// 
/// Demonstrates an approval workflow scenario where steps may be blocked due to policy decisions.
/// 
/// Scenario Details:
/// - Starts a new workflow with multiple steps using <see cref="StartApprovalMutation"/>.
/// - Sequentially attempts to approve each step using <see cref="ApproveStepMutation"/>.
/// - Uses <see cref="MutationContext.User"/> to simulate individual users performing approvals.
/// - Logs success or policy-based rejections for each step.
/// - Works with <see cref="ApprovalWorkflowState"/> which tracks the status of each step, who approved or rejected it.
/// 
/// Key Steps:
/// 1. Initialize <see cref="ApprovalWorkflowState"/> as empty workflow state.
/// 2. Start workflow with predefined steps using <see cref="StartApprovalMutation"/> and <see cref="MutationContext.System"/>.
/// 3. Sequentially attempt to approve each step using different user contexts.
/// 4. Evaluate each mutation result for success or policy denial.
/// 5. Print final workflow state with detailed approval/rejection info.
/// 
/// Example Use Case:
/// - Testing policy enforcement in approval workflows.
/// - Simulating rejected steps due to validation rules, risk controls, or approval policies.
/// - Demonstrating auditability by showing which users attempted approvals and why any were blocked.
/// 
/// Notes:
/// - Policy decisions may block specific steps and are logged per mutation.
/// - The scenario can be extended to mix approvals and rejections to test complex workflows.
/// - Correlation IDs and metadata can be used to trace and audit workflow operations.
/// </summary>
internal static class RejectedScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Rejected Scenario ===");

        var state = new ApprovalWorkflowState();

        var systemCtx = MutationContext.System("Start workflow", correlationId: "workflow-456");
        var start = new StartApprovalMutation("initiator", ["Step1", "Step2", "Step3"], systemCtx);
        var result = await engine.ExecuteAsync(start, state);

        if (!result.IsSuccess || result.NewState == null)
        {
            Console.WriteLine("✗ Failed to start workflow.");
            return;
        }

        state = result.NewState;

        var approvers = new[] { "alice", "bob", "carol" };

        for (var i = 0; i < state.Steps.Count; i++)
        {
            var userCtx = MutationContext.User(approvers[i], reason: $"Approve Step{i}");

            var approve = new ApproveStepMutation(i, approvers[i], userCtx);
            var res = await engine.ExecuteAsync(approve, state);

            if (res.IsSuccess)
            {
                state = res.NewState!;
                Console.WriteLine($"✓ Step {i} approved by {approvers[i]}");
            }
            else
            {
                Console.WriteLine($"✗ Step {i} blocked for {approvers[i]}:");
                foreach (var dec in res.PolicyDecisions)
                    Console.WriteLine($"  Policy: {dec.PolicyName} – {dec.Reason}");
            }
        }

        Console.WriteLine("Workflow final state:");
        for (var i = 0; i < state.Steps.Count; i++)
        {
            var s = state.Steps[i];
            Console.WriteLine($"  Step{i}: {s.Status} by {(s.ApprovedBy ?? s.RejectedBy ?? "-")}");
        }
    }
}