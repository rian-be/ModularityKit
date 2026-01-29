using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Runtime;
using ModularityKit.Mutators.Runtime.Loggers;
using Mutators.Examples.BillingQuotas.Policies;
using Mutators.Examples.IamRoles.Policies;
using Mutators.Examples.WorkflowApprovals.Policies;
using TwoManApprovalPolicy = Mutators.Examples.IamRoles.Policies.RequireTwoManApprovalPolicy;

namespace Mutators;

internal static class Program
{
    private static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddMutators(MutationEngineOptions.Strict, addDefaultLoggingInterceptor: true);

        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IMutationEngine>();
        
        Console.WriteLine("=== ModularityKit.Mutators - Complete Example ===\n");
        
        // BillingQuotas
        engine.RegisterPolicy(new MaxQuotaPolicy());
        engine.RegisterPolicy(new PreventNegativeQuotaPolicy());
        
        // FeatureFlags
        //engine.RegisterPolicy(new BusinessHoursPolicy());
        engine.RegisterPolicy(new TwoManApprovalPolicy());

        // IamRoles
        engine.RegisterPolicy(new PreventLastAdminRemovalPolicy());
        engine.RegisterPolicy(new RequireTwoManApprovalPolicy());
        
        // WorkflowApprovals
        engine.RegisterPolicy(new EnforceOrderPolicy());
        engine.RegisterPolicy(new RequireManagerApprovalPolicy());

        await Examples.FeatureFlags.Scenarios.EnableNewCheckoutScenario.Run(engine);
        await Examples.FeatureFlags.Scenarios.DisableLegacyCheckoutScenario.Run(engine);
        await Examples.FeatureFlags.Scenarios.BatchFeatureToggleScenario.Run(engine);

        await Examples.WorkflowApprovals.Scenarios.HappyPathScenario.Run(engine);
        await Examples.WorkflowApprovals.Scenarios.RejectedScenario.Run(engine);
         
        await Examples.BillingQuotas.Scenarios.EmergencyIncreaseScenario.Run(engine);
        await Examples.BillingQuotas.Scenarios.MonthlyResetScenario.Run(engine);
         
        await Examples.IamRoles.Scenarios.GrantAdminScenario.Run(engine);
        await Examples.IamRoles.Scenarios.RevokeAdminScenario.Run(engine);
        await Examples.IamRoles.Scenarios.BatchRoleMigrationScenario.Run(engine);

        var historys = await engine.GetHistoryAsync(stateId: "EnableNewCheckout");
        MutationHistoryLogger.LogHistory(historys);
        
        Console.WriteLine("\n METRICS & STATISTICS");

        var stats = await engine.GetStatisticsAsync();

        Console.WriteLine($"\n Mutation Statistics:");
        Console.WriteLine($"  Total executed: {stats.TotalExecuted}");

        Console.WriteLine($"\n Performance Metrics:");
        Console.WriteLine($"  Average execution time: {stats.AverageExecutionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Median execution time: {stats.MedianExecutionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  P95 execution time: {stats.P95ExecutionTime.TotalMilliseconds:F2} ms");
    }
}
