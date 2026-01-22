namespace ModularityKit.Mutators.Abstractions.Audit;

/// <summary>
/// Audits mutation operations. Responsible for recording, storing, and retrieving
/// mutation audit entries for traceability and compliance.
/// </summary>
/// <remarks>
/// Mutation auditors track changes applied to state objects via mutations. They can be used
/// for compliance, debugging, analytics, or security reviews. Implementations may persist
/// audit entries in databases, event stores, or logging systems.
/// </remarks>
public interface IMutationAuditor
{
    /// <summary>
    /// Records an audit entry for a mutation.
    /// </summary>
    /// <param name="entry">The mutation audit entry to store.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task AuditAsync(
        MutationAuditEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit log entries for a specific state object.
    /// </summary>
    /// <param name="stateId">Identifier of the state object.</param>
    /// <param name="from">Optional start timestamp for filtering entries.</param>
    /// <param name="to">Optional end timestamp for filtering entries.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Read-only list of mutation audit entries.</returns>
    Task<IReadOnlyList<MutationAuditEntry>> GetAuditLogAsync(
        string stateId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);
}
