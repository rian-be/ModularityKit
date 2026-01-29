using ModularityKit.Mutators.Abstractions.History;

namespace ModularityKit.Mutators.Runtime.Loggers;

public static class MutationHistoryLogger
{
    public static void LogHistory(MutationHistory history)
    {
        Console.WriteLine($"Total mutations in history: {history.TotalMutations}");
        Console.WriteLine($"First mutation: {history.FirstMutationAt}");
        Console.WriteLine($"Last mutation: {history.LastMutationAt}\n");

        Console.WriteLine("Timeline:");
        foreach (var entry in history.Entries)
        {
            var actorId = entry.Context.ActorId ?? "Unknown";
            var actorName = entry.Context.ActorName ?? actorId;

            Console.WriteLine(
                $"  [{entry.Timestamp:HH:mm:ss}] {entry.Intent.OperationName} by {actorName} ({actorId}) " +
                $"(Reason: {entry.Context.Reason ?? "N/A"}) - " +
                $"Changes: {entry.Changes.Count,2}, Execution: {entry.ExecutionTime.TotalMilliseconds,6:F2}ms");
        }
    }
}