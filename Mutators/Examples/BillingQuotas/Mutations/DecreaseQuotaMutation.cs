using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Mutations;

/// <summary>
/// Mutation that decreases the quota for specific user by specified amount.
/// </summary>
internal sealed record DecreaseQuotaMutation(
    string UserId,
    int Amount,
    MutationContext Context
) : IMutation<QuotaState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "DecreaseQuota",
        Category = "Billing",
        RiskLevel = MutationRiskLevel.High,
        Description = "Decrease user quota by given amount"
    };

    public ValidationResult Validate(QuotaState state)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(UserId))
            result.AddError("UserId", "UserId cannot be empty");

        if (Amount <= 0)
            result.AddError("Amount", "Amount must be positive");

        if (state.UserQuotas.GetValueOrDefault(UserId) < Amount)
            result.AddError("Amount", "Cannot decrease below zero");

        return result;
    }

    public MutationResult<QuotaState> Apply(QuotaState state)
    {
        var quotas = state.UserQuotas.ToDictionary(kv => kv.Key, kv => kv.Value);
        quotas[UserId] = quotas.GetValueOrDefault(UserId) - Amount;

        var newState = state with { UserQuotas = quotas };

        var changes = ChangeSet.Single(
            StateChange.Modified($"UserQuotas.{UserId}", null, quotas[UserId])
        );

        return MutationResult<QuotaState>.Success(newState, changes);
    }

    public MutationResult<QuotaState> Simulate(QuotaState state) => Apply(state);
}