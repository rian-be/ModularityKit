using Core.Features.Context.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Core.Context.Services;

namespace Polygon.Core.Context;

/// <summary>
/// Executes a complete demonstration of context isolation, security, and 
/// multi-accessor behavior using trusted and untrusted services.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Shows how trusted components consume full mutable context via <see cref="IContextAccessor{TContext}"/>.</item>
/// <item>Shows how untrusted components consume restricted <see cref="IReadOnlyContext"/> snapshots.</item>
/// <item>Demonstrates scoping execution through <see cref="IContextManager{TContext}"/>.</item>
/// <item>Illustrates metadata, feature flags, and correlation propagation.</item>
/// </list>
/// </remarks>
public class TestingContext(
    IContextManager<MyContext> contextManager,
    IContextAccessor<MyContext> fullContextAccessor,
    IContextAccessor<IReadOnlyContext> readOnlyContextAccessor)
{
    /// <summary>
    /// Executes a full demonstration of context management capabilities, 
    /// including trusted and untrusted service interactions within a scoped context.
    /// </summary>
    /// <returns>A task that completes when the demo has finished executing.</returns>
    public async Task RunDemo()
    {
        // Create services
        var trustedService = new TrustedService(fullContextAccessor);
        var untrustedService = new UntrustedService(readOnlyContextAccessor);

        // Create context
        var myContext = MyContext.Create(
            userId: "user-123",
            tenantId: "tenant-456",
            userEmail: "alice@company.com",
            roles: ["Admin", "Developer", "ApiUser"]
        );

        myContext.Metadata["Source"] = "ConsoleApp";
        myContext.Metadata["Environment"] = "Development";
        myContext.Metadata["RequestId"] = Guid.NewGuid().ToString();
        myContext.Metadata["ClientIp"] = "192.168.1.100";

        // Enable features
        myContext.EnabledFeatures.Add("AdvancedLogging");
        myContext.EnabledFeatures.Add("BetaFeatures");
        myContext.EnabledFeatures.Add("ExperimentalAPI");

        Console.WriteLine("Created Context:");
        Console.WriteLine($"  ID: {myContext.Id}");
        Console.WriteLine($"  User: {myContext.UserId}");
        Console.WriteLine($"  Tenant: {myContext.TenantId}");
        Console.WriteLine($"  Correlation: {myContext.CorrelationId}");
        Console.WriteLine();

        await contextManager.ExecuteInContext(myContext, async () =>
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          TRUSTED SERVICE (Full Access)                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            trustedService.DoWork();
            
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       UNTRUSTED SERVICE (Read-Only Access)                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            untrustedService.ProcessData();
            
            await Task.CompletedTask;
        });

        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    Demo Complete                             ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
    }

    /// <summary>
    /// Creates a new <see cref="TestingContext"/> instance using dependency injection.
    /// </summary>
    /// <param name="serviceProvider">The application's root service provider.</param>
    /// <returns>A configured <see cref="TestingContext"/> instance.</returns>
    public static TestingContext Create(IServiceProvider serviceProvider)
    {
        var contextManager = serviceProvider.GetRequiredService<IContextManager<MyContext>>();
        var fullAccessor = serviceProvider.GetRequiredService<IContextAccessor<MyContext>>();
        var readOnlyAccessor = serviceProvider.GetRequiredService<IContextAccessor<IReadOnlyContext>>();

        return new TestingContext(contextManager, fullAccessor, readOnlyAccessor);
    }
}