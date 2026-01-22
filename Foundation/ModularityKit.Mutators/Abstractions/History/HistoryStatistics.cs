namespace ModularityKit.Mutators.Abstractions.History;

/// <summary>
/// Provides aggregated statistics for a mutation history.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HistoryStatistics"/> summarizes key metrics about the mutations applied to a state,
/// such as the total number of mutations, unique actors involved, distribution by category,
/// and average number of changes per mutation.
/// </para>
/// <para>
/// This class is typically returned by <see cref="MutationHistory.GetStatistics"/> and is used
/// for auditing, reporting, and monitoring purposes.
/// </para>
/// </remarks>
public sealed class HistoryStatistics
{
    /// <summary>
    /// Total number of mutations in the history.
    /// </summary>
    public int TotalMutations { get; init; }

    /// <summary>
    /// Number of distinct actors who performed mutations.
    /// </summary>
    public int UniqueActors { get; init; }

    /// <summary>
    /// Count of mutations grouped by their category.
    /// </summary>
    public IReadOnlyDictionary<string, int> MutationsByCategory { get; init; }
        = new Dictionary<string, int>();

    /// <summary>
    /// Average number of state changes per mutation.
    /// </summary>
    public double AverageChangesPerMutation { get; init; }
}
