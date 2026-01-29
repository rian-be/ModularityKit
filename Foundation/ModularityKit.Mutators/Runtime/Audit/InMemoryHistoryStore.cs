using ModularityKit.Mutators.Abstractions.History;

namespace ModularityKit.Mutators.Runtime.Audit;

/// <summary>
/// An in-memory implementation of <see cref="IMutationHistoryStore"/> suitable for testing and development.
/// </summary>
/// <remarks>
/// <para>
/// All history entries are stored in memory. This implementation is **not suitable for production** as it does
/// not persist entries beyond the lifetime of the process.
/// </para>
/// <para>
/// Thread-safe: all public methods use locking to prevent concurrent access issues.
/// </para>
/// </remarks>
internal sealed class InMemoryHistoryStore : IMutationHistoryStore
{
    private readonly Dictionary<string, List<MutationHistoryEntry>> _history = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Stores a mutation history entry for a state.
    /// </summary>
    /// <param name="entry">The mutation history entry to store.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public Task StoreAsync(
        MutationHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var stateId = ExtractStateId(entry);

            if (!_history.ContainsKey(stateId))
                _history[stateId] = new List<MutationHistoryEntry>();

            _history[stateId].Add(entry);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the full mutation history for a given state.
    /// </summary>
    /// <param name="stateId">The ID of the state.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="MutationHistory"/> containing all entries for the state.</returns>
    public Task<MutationHistory> GetHistoryAsync(
        string stateId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var entries = _history.TryGetValue(stateId, out var list)
                ? list.OrderBy(e => e.Timestamp).ToList()
                : new List<MutationHistoryEntry>();

            return Task.FromResult(new MutationHistory
            {
                StateId = stateId,
                Entries = entries
            });
        }
    }

    /// <summary>
    /// Retrieves the mutation history for a state within a specified time range.
    /// </summary>
    /// <param name="stateId">The ID of the state.</param>
    /// <param name="from">Start of the time range.</param>
    /// <param name="to">End of the time range.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="MutationHistory"/> containing all entries in the range.</returns>
    public Task<MutationHistory> GetHistoryRangeAsync(
        string stateId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var entries = _history.TryGetValue(stateId, out var list)
                ? list.Where(e => e.Timestamp >= from && e.Timestamp <= to)
                      .OrderBy(e => e.Timestamp)
                      .ToList()
                : new List<MutationHistoryEntry>();

            return Task.FromResult(new MutationHistory
            {
                StateId = stateId,
                Entries = entries
            });
        }
    }

    /// <summary>
    /// Retrieves the most recent <paramref name="count"/> mutation history entries for a state.
    /// </summary>
    /// <param name="stateId">The ID of the state.</param>
    /// <param name="count">The maximum number of recent entries to return.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of <see cref="MutationHistoryEntry"/> ordered from newest to oldest.</returns>
    public Task<IReadOnlyList<MutationHistoryEntry>> GetRecentAsync(
        string stateId,
        int count,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var entries = _history.TryGetValue(stateId, out var list)
                ? list.OrderByDescending(e => e.Timestamp).Take(count).ToList()
                : [];

            return Task.FromResult<IReadOnlyList<MutationHistoryEntry>>(entries);
        }
    }

    /// <summary>
    /// Clears all in-memory history entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _history.Clear();
        }
    }

    /// <summary>
    /// Extracts a state ID from a mutation history entry.
    /// </summary>
    /// <param name="entry">The entry to extract from.</param>
    /// <returns>The extracted state ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when StateId is missing.</exception>
    private string ExtractStateId(MutationHistoryEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.StateId))
        {
            throw new InvalidOperationException("Stable StateId is required for history lookups.");
        }

        return entry.StateId;
    }
}
