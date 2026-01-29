using ModularityKit.Mutators.Abstractions.Results;

namespace ModularityKit.Mutators.Runtime.Loggers;

public static class MutationResultLogger
{
    public static void LogBatch<TState>(IEnumerable<MutationResult<TState>> results)
    {
        foreach (var result in results)
        {
            Console.WriteLine($"[ExecutionId: {result.CompletedAt:HH:mm:ss}] {result.NewState?.GetType().Name ?? "Mutation"}");
            
            if (result.Exception != null)
            {
                Console.WriteLine("✗ Mutation failed with exception:");
                Console.WriteLine($"  {result.Exception.Message}");
                continue;
            }

            if (result.PolicyDecisions.Any())
            {
                Console.WriteLine("✗ Mutation blocked by policy:");
                foreach (var decision in result.PolicyDecisions)
                    Console.WriteLine($"  Policy: {decision.PolicyName}, Reason: {decision.Reason}");
                continue;
            }

            if (!result.ValidationResult.IsValid)
            {
                Console.WriteLine("✗ Mutation failed validation:");
                foreach (var error in result.ValidationResult.Errors)
                    Console.WriteLine($"  {error.Path}: {error.Message}");
                continue;
            }

            if (!result.IsSuccess) continue;
            
            Console.WriteLine($"  ✓ Success, changes: {result.Changes.Count}");
            foreach (var change in result.Changes.Changes)
                Console.WriteLine($"    - {change.Path}: {change.OldValue} -> {change.NewValue}");
        }
    }
}