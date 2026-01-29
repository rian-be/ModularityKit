using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Policies;
using Mutators.Examples.BillingQuotas.Mutations;
using Mutators.Examples.BillingQuotas.State;

namespace Mutators.Examples.BillingQuotas.Policies;

/// <summary>
/// Policy ensuring that user's quota never drops below zero.
/// Blocks any <see cref="DecreaseQuotaMutation"/> that would result in negative quota.
/// </summary>
public sealed class PreventNegativeQuotaPolicy : IMutationPolicy<QuotaState>
{
    public string Name => "PreventNegativeQuotaPolicy";
    public int Priority => 100;
    public string Description => "Prevents user quota from going below zero.";
    
    public PolicyDecision Evaluate(IMutation<QuotaState> mutation, QuotaState state)
    {
        if (mutation is not DecreaseQuotaMutation dec) return PolicyDecision.Allow();
        
        var current = state.UserQuotas.GetValueOrDefault(dec.UserId);
        if (current - dec.Amount < 0)
            return PolicyDecision.Deny(
                $"Quota cannot go below zero (current: {current}, decrease: {dec.Amount})");

        return PolicyDecision.Allow();
    }
}