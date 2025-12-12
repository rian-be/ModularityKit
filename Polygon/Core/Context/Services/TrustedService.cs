using Core.Features.Context.Abstractions;

namespace Polygon.Core.Context.Services;

/// <summary>
/// Represents a trusted application service with full access to the execution context,
/// including identity, roles, tenancy, metadata, and sensitive operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Operates on <see cref="MyContext"/> retrieved via <see cref="IContextAccessor{TContext}"/>.</item>
/// <item>Intended only for high-trust layers such as internal pipelines, system components, 
/// security-cleared code paths, or infrastructure orchestration.</item>
/// <item>Has permission to mutate context state, modify metadata, and invoke sensitive operations.</item>
/// <item>Must never be exposed to untrusted or sandboxed code paths.</item>
/// </list>
/// </remarks>
public class TrustedService(IContextAccessor<MyContext> contextAccessor)
{
    /// <summary>
    /// Performs diagnostic operations against the full context, demonstrating
    /// identity, authorization, feature flags, metadata access, and sensitive operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Reads all identity, tenant, correlation, and trace information.</item>
    /// <item>Evaluates role membership and feature enablement.</item>
    /// <item>Enumerates and mutates context metadata.</item>
    /// <item>Invokes sensitive operations on <see cref="MyContext"/>, which must not be accessible to untrusted components.</item>
    /// </list>
    /// </remarks>
    public void DoWork()
    {
        var ctx = contextAccessor.RequireCurrent();

        // Full access to context internals
        Console.WriteLine("=== Trusted Service Has Full Access ===");
        Console.WriteLine($"Context ID: {ctx.Id}");
        Console.WriteLine($"User ID: {ctx.UserId}");
        Console.WriteLine($"User Email: {ctx.UserEmail}");
        Console.WriteLine($"Tenant ID: {ctx.TenantId}");
        Console.WriteLine($"Correlation: {ctx.CorrelationId}");
        Console.WriteLine($"Created: {ctx.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        // Role checks
        Console.WriteLine("\n=== Role Checks ===");
        Console.WriteLine($"Has Admin role: {ctx.HasRole("Admin")}");
        Console.WriteLine($"Has Developer role: {ctx.HasRole("Developer")}");
        Console.WriteLine($"Has User role: {ctx.HasRole("User")}");
        Console.WriteLine($"All roles: {string.Join(", ", ctx.Roles)}");

        // Feature flags
        Console.WriteLine("\n=== Feature Flags ===");
        Console.WriteLine($"AdvancedLogging enabled: {ctx.IsFeatureEnabled("AdvancedLogging")}");
        Console.WriteLine($"BetaFeatures enabled: {ctx.IsFeatureEnabled("BetaFeatures")}");
        Console.WriteLine($"LegacyMode enabled: {ctx.IsFeatureEnabled("LegacyMode")}");
        Console.WriteLine($"All features: {string.Join(", ", ctx.EnabledFeatures)}");

        // Metadata
        Console.WriteLine("\n=== Metadata ===");
        Console.WriteLine($"Metadata entries: {ctx.Metadata.Count}");
        foreach (var (key, value) in ctx.Metadata)
        {
            Console.WriteLine($"  {key}: {value}");
        }

        // Sensitive operations
        ctx.DoSomething();
        ctx.ModifyTenant("new-tenant-123");

        Console.WriteLine("\n=== Calling Sensitive Operations ===");
        ctx.DoSomething();
        ctx.ModifyTenant("new-tenant-123");

        // Mutate metadata
        ctx.Metadata["ProcessedBy"] = "TrustedService";
        ctx.Metadata["ProcessedAt"] = DateTimeOffset.UtcNow;

        Console.WriteLine($"\nMetadata after processing: {ctx.Metadata.Count} entries");
    }
}
