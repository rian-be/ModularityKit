using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using Mutators.Examples.WorkflowApprovals.Mutations;
using Mutators.Examples.WorkflowApprovals.State;

namespace Mutators.Examples.WorkflowApprovals.Scenarios;

/// <summary>
/// HappyPathScenario
/// 
/// Demonstrates a "happy path" execution of an approval workflow using <see cref="IMutationEngine"/> 
/// and <see cref="IMutation{TState}"/> where all steps are approved successfully.
/// 
/// Scenario Details:
/// - Starts a new approval workflow using <see cref="StartApprovalMutation"/>.
/// - Sequentially approves each step in the workflow using <see cref="ApproveStepMutation"/>.
/// - Uses <see cref="MutationContext.System"/> to provide system metadata and correlation ID for audit purposes.
/// - Logs success and failure for each mutation.
/// - Works with <see cref="ApprovalWorkflowState"/> which tracks workflow steps and their approval status.
/// 
/// Key Steps:
/// 1. Initialize <see cref="ApprovalWorkflowState"/> as empty workflow state.
/// 2. Start workflow with multiple steps via <see cref="StartApprovalMutation"/>.
/// 3. Sequentially approve each step using predefined approvers.
/// 4. Update workflow state after each mutation.
/// 5. Print final state of all steps, including approved or rejected information.
/// 
/// Example Use Case:
/// - Simulating normal workflow progression for testing or documentation purposes.
/// - Verifying that policies allow proper step approvals in sequence.
/// - Demonstrating auditability and traceability with correlation IDs and system metadata.
/// 
/// Notes:
/// - If a mutation is blocked due to a policy decision, the scenario logs the blocking reason.
/// - This scenario assumes all mutations succeed under normal conditions.
/// - Can be extended with rejection scenarios or conditional approvals for more complex workflows.
/// </summary>
internal static class HappyPathScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Happy Path Scenario ===");

        var state = new ApprovalWorkflowState();

        var ctx = MutationContext.System("Start workflow", correlationId: "workflow-123");

        var start = new StartApprovalMutation("initiator", ["Step1", "Step2", "Step3"], ctx);
        var result = await engine.ExecuteAsync(start, state);
        state = result.NewState!;
        
        var approvers = new[] { "alice", "bob", "alice" };

        for (var i = 0; i < state.Steps.Count; i++)
        {
            var approve = new ApproveStepMutation(i, approvers[i], ctx);
            var res = await engine.ExecuteAsync(approve, state);
            if (res.IsSuccess)
                state = res.NewState!;
            else
            {
                Console.WriteLine($"✗ Step {i} blocked:");
                foreach (var dec in res.PolicyDecisions)
                    Console.WriteLine($"  {dec.PolicyName} – {dec.Reason}");
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