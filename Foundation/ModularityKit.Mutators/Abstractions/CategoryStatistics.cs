namespace ModularityKit.Mutators.Abstractions;

/// <summary>
/// Collects execution statistics for a specific mutation category.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CategoryName"/> represents the name of the mutation category.
/// </para>
/// <para>
/// <see cref="Count"/> is the total number of mutations in this category.
/// <see cref="SuccessCount"/> is the number of successful mutations,
/// <see cref="FailureCount"/> is the number of failed mutations.
/// </para>
/// <para>
/// <see cref="AverageExecutionTime"/> indicates the average execution time for mutations in this category.
/// </para>
/// </remarks>
public sealed class CategoryStatistics
{
    /// <summary>
    /// The name of the mutation category.
    /// </summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Total number of mutations in this category.
    /// </summary>
    public long Count { get; init; }

    /// <summary>
    /// Number of successful mutations in this category.
    /// </summary>
    public long SuccessCount { get; init; }

    /// <summary>
    /// Number of failed mutations in this category.
    /// </summary>
    public long FailureCount { get; init; }

    /// <summary>
    /// Average execution time of mutations in this category.
    /// </summary>
    public TimeSpan AverageExecutionTime { get; init; }
}
