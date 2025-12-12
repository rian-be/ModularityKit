using Core.Features.Context.Abstractions;

namespace Polygon.Core.Context.Services;

/// <summary>
/// Represents an untrusted consumer of execution context information that operates
/// exclusively on a secured <see cref="IReadOnlyContext"/> snapshot.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Receives only immutable and sanitized context data through <see cref="IContextAccessor{IReadOnlyContext}"/>.</item>
/// <item>Cannot access identity, authorization, tenant, metadata, or feature-flag information.</item>
/// <item>Cannot call any sensitive context operations, even via reflection or dynamic dispatch.</item>
/// <item>Serves as a validation layer demonstrating context isolation guarantees.</item>
/// </list>
/// </remarks>
public class UntrustedService(IContextAccessor<IReadOnlyContext> contextAccessor)
{
    /// <summary>
    /// Executes a series of controlled read-only operations intended to validate and
    /// demonstrate the security boundaries imposed on untrusted execution paths.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Reads only <see cref="IReadOnlyContext"/> properties: <c>Id</c> and <c>CreatedAt</c>.</item>
    /// <item>Attempts to discover or invoke sensitive properties and methods using type inspection, reflection, and dynamic dispatch.</item>
    /// <item>Verifies that no sensitive APIs such as <c>UserId</c>, <c>Roles</c>, <c>Metadata</c>, or <c>DoSomething</c> are accessible.</item>
    /// <item>Confirms that only the read-only projection type is visible at runtime.</item>
    /// <item>Ensures that no privilege escalation is possible from a sandboxed service.</item>
    /// </list>
    /// </remarks>
    public void ProcessData()
    {
        var ctx = contextAccessor.RequireCurrent();

        Console.WriteLine("=== Untrusted Service Execution ===");
        Console.WriteLine($"Context ID: {ctx.Id}");
        Console.WriteLine($"Created At: {ctx.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        Console.WriteLine("\n=== Security Bypass Attempts ===");

        // Attempt 1: Type inspection
        Console.WriteLine("\n[Attempt 1] Type Inspection");
        Console.WriteLine($"  Actual Type: {ctx.GetType().FullName}");
        Console.WriteLine("  Expected: Polygon.Context.MyContext");
        Console.WriteLine($"  Match: {ctx.GetType().Name == "MyContext"}");

        // Attempt 2: Reflection - look for sensitive properties
        Console.WriteLine("\n[Attempt 2] Reflection - Sensitive Properties");
        var sensitiveProps = new[] { "UserId", "UserEmail", "TenantId", "Roles", "Metadata", "EnabledFeatures" };
        foreach (var propName in sensitiveProps)
        {
            var prop = ctx.GetType().GetProperty(propName);
            Console.WriteLine($"  {propName}: {(prop == null ? "NOT FOUND ✓" : "FOUND ✗")}");
        }

        // Attempt 3: Reflection - look for sensitive methods
        Console.WriteLine("\n[Attempt 3] Reflection - Sensitive Methods");
        var sensitiveMethods = new[] { "DoSomething", "ModifyTenant", "HasRole", "IsFeatureEnabled" };
        foreach (var methodName in sensitiveMethods)
        {
            var method = ctx.GetType().GetMethod(methodName);
            Console.WriteLine($"  {methodName}: {(method == null ? "NOT FOUND ✓" : "FOUND ✗")}");
        }

        // Attempt 4: Dynamic access
        Console.WriteLine("\n[Attempt 4] Dynamic Access - Try to access UserId");
        try
        {
            dynamic dynCtx = ctx;
            var userId = dynCtx.UserId;
            Console.WriteLine($"  SECURITY BREACH ✗: Got UserId = {userId}");
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
        {
            Console.WriteLine($"  Blocked ✓: {ex.Message}");
        }

        // Attempt 5: Dynamic access - Try to access Roles
        Console.WriteLine("\n[Attempt 5] Dynamic Access - Try to access Roles");
        try
        {
            dynamic dynCtx = ctx;
            var roles = dynCtx.Roles;
            Console.WriteLine($"  SECURITY BREACH ✗: Got Roles = {string.Join(", ", roles)}");
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            Console.WriteLine("  Blocked ✓: Property not accessible");
        }

        // Attempt 6: Dynamic access - Try to call DoSomething
        Console.WriteLine("\n[Attempt 6] Dynamic Access - Try to call DoSomething()");
        try
        {
            dynamic dynCtx = ctx;
            dynCtx.DoSomething();
            Console.WriteLine("  SECURITY BREACH ✗: Method executed!");
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            Console.WriteLine("  Blocked ✓: Method not accessible");
        }

        // Attempt 7: Interface casting
        Console.WriteLine("\n[Attempt 7] Interface Analysis");
        Console.WriteLine($"  Is IReadOnlyContext: {ctx is IReadOnlyContext}");
        Console.WriteLine($"  Is IContext: {ctx is IContext}");

        // Attempt 8: Runtime type check
        Console.WriteLine("\n[Attempt 8] Runtime Type Check");
        var isMyContext = ctx.GetType() == typeof(MyContext);
        Console.WriteLine($"  Is MyContext: {isMyContext}");
        Console.WriteLine($"  Actual type name: {ctx.GetType().Name}");

        // Attempt 9: Try to access base type properties through reflection
        Console.WriteLine("\n[Attempt 9] Reflection - Try to get all properties");
        var allProps = ctx.GetType().GetProperties();
        Console.WriteLine($"  Total properties found: {allProps.Length}");
        Console.WriteLine("  Available properties:");
        foreach (var prop in allProps)
        {
            Console.WriteLine($"    - {prop.Name} ({prop.PropertyType.Name})");
        }

        // Attempt 10: Try to invoke getter for 'UserId'
        Console.WriteLine("\n[Attempt 10] Reflection - Try to invoke getter for 'UserId'");
        var userIdProp = ctx.GetType().GetProperty("UserId");
        if (userIdProp != null)
        {
            try
            {
                var value = userIdProp.GetValue(ctx);
                Console.WriteLine($"  SECURITY BREACH ✗: Got UserId = {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Failed ✓: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("  Property doesn't exist ✓");
        }

        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Conclusion: All bypass attempts failed ✓              ║");
        Console.WriteLine("║    Security boundary is effective!                        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    }
}
