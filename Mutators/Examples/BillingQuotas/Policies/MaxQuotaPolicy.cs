using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.BillingQuotas.Mutations;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Policies;

/// <summary>
/// Policy ensuring that user's quota cannot exceed specified maximum.
/// </summary>
public sealed class MaxQuotaPolicy(int maxQuota = 100) : IMutationPolicy<QuotaState>
{
    public string Name => "MaxQuotaPolicy";
    public int Priority => 100;
    public string Description => $"Ensures that user quota does not exceed {maxQuota}.";

    public PolicyDecision Evaluate(IMutation<QuotaState> mutation, QuotaState state)
    {
        if (mutation is not IncreaseQuotaMutation inc) return PolicyDecision.Allow();
       
        var current = state.UserQuotas.GetValueOrDefault(inc.UserId);
        if (current + inc.Amount > maxQuota)
            return PolicyDecision.Deny(
                $"Quota cannot exceed {maxQuota} (current: {current}, increase: {inc.Amount})");

        return PolicyDecision.Allow();
    }
}