using ModularityKit.Mutators.Abstractions.Audit;

namespace ModularityKit.Mutators.Runtime.Audit;

/// <summary>
/// An in-memory implementation of <see cref="IMutationAuditor"/> suitable for testing and development.
/// </summary>
/// <remarks>
/// <para>
/// All audit entries are stored in memory. This implementation is **not suitable for production** as it does
/// not persist entries beyond the lifetime of the process.
/// </para>
/// <para>
/// Thread-safe: all public methods use locking to prevent concurrent access issues.
/// </para>
/// </remarks>
internal sealed class InMemoryAuditor : IMutationAuditor
{
    private readonly List<MutationAuditEntry> _entries = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// Records a mutation audit entry in memory.
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public Task AuditAsync(
        MutationAuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the audit log for a given state within an optional time range.
    /// </summary>
    /// <param name="stateId">The ID of the state to query.</param>
    /// <param name="from">Optional start of the time range.</param>
    /// <param name="to">Optional end of the time range.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of <see cref="MutationAuditEntry"/> matching the query.</returns>
    public Task<IReadOnlyList<MutationAuditEntry>> GetAuditLogAsync(
        string stateId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _entries.Where(e => e.StateId == stateId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            return Task.FromResult<IReadOnlyList<MutationAuditEntry>>(
                [.. query]);
        }
    }

    /// <summary>
    /// Returns all stored audit entries in memory.
    /// </summary>
    public IReadOnlyList<MutationAuditEntry> GetAllEntries()
    {
        lock (_lock)
        {
            return [.. _entries];
        }
    }

    /// <summary>
    /// Clears all in-memory audit entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
