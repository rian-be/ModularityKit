using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Mutations;

/// <summary>
/// Mutation that increases the quota for specific user by given amount.
/// </summary>
internal sealed record IncreaseQuotaMutation(
    string UserId,
    int Amount,
    MutationContext Context
) : IMutation<QuotaState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "IncreaseQuota",
        Category = "Billing",
        RiskLevel = MutationRiskLevel.Medium,
        Description = "Increase user quota by given amount"
    };

    public ValidationResult Validate(QuotaState state)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(UserId))
            result.AddError("UserId", "UserId cannot be empty");

        if (Amount <= 0)
            result.AddError("Amount", "Amount must be positive");

        return result;
    }

    public MutationResult<QuotaState> Apply(QuotaState state)
    {
        var quotas = state.UserQuotas.ToDictionary(kv => kv.Key, kv => kv.Value);
        quotas[UserId] = quotas.GetValueOrDefault(UserId) + Amount;

        var newState = state with { UserQuotas = quotas };

        var changes = ChangeSet.Single(
            StateChange.Modified($"UserQuotas.{UserId}", null, quotas[UserId])
        );

        return MutationResult<QuotaState>.Success(newState, changes);
    }

    public MutationResult<QuotaState> Simulate(QuotaState state) => Apply(state);
}