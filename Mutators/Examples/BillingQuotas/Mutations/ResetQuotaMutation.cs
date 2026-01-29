using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Mutations;

/// <summary>
/// Mutation that resets user's quota to zero.
/// </summary>
internal sealed record ResetQuotaMutation(
    string UserId,
    MutationContext Context
) : IMutation<QuotaState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "ResetQuota",
        Category = "Billing",
        RiskLevel = MutationRiskLevel.High,
        Description = "Reset user quota to zero"
    };

    public ValidationResult Validate(QuotaState state)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(UserId))
            result.AddError("UserId", "UserId cannot be empty");

        return result;
    }

    public MutationResult<QuotaState> Apply(QuotaState state)
    {
        var quotas = state.UserQuotas.ToDictionary(kv => kv.Key, kv => kv.Value);
        quotas[UserId] = 0;

        var newState = state with { UserQuotas = quotas };

        var changes = ChangeSet.Single(
            StateChange.Modified($"UserQuotas.{UserId}", null, 0)
        );

        return MutationResult<QuotaState>.Success(newState, changes);
    }

    public MutationResult<QuotaState> Simulate(QuotaState state) => Apply(state);
}