namespace Mutators.Examples.BillingQuotas.State;

/// <summary>
/// Represents user quota state in billing system.
/// </summary>
/// <remarks>
/// Holds read only mapping of user IDs to their remaining quota units.
/// Immutable and safe for concurrent use.
/// </remarks>
public sealed record QuotaState
{
    public IReadOnlyDictionary<string, int> UserQuotas { get; init; }
        = new Dictionary<string, int>();
}