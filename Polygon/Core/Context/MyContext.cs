using Core.Features.Context.Abstractions;

namespace Polygon.Core.Context;

/// <summary>
/// Execution context carrying identity, tenancy, tracing, and feature metadata 
/// for trusted application components. Designed for full-trust environments 
/// where mutation of contextual state is permitted.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IContext"/> as the authoritative full-access context model.</item>
/// <item>Contains user identity, tenant classification, roles, and organization information.</item>
/// <item>Supports correlation IDs and distributed tracing identifiers.</item>
/// <item>Provides extensibility via <see cref="Metadata"/> and <see cref="EnabledFeatures"/>.</item>
/// <item>Intended only for trusted subsystems; untrusted layers must use <see cref="IReadOnlyContext"/> snapshots.</item>
/// </list>
/// </remarks>
public sealed class MyContext(
    string id,
    string userId,
    string tenantId,
    string correlationId,
    DateTimeOffset? createdAt = null,
    string? userEmail = null,
    string[]? roles = null,
    string? organizationId = null,
    string? traceId = null,
    string? spanId = null)
    : IContext
{
    /// <inheritdoc />
    public string Id { get; } = id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; } = createdAt ?? DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the authenticated user identifier associated with this execution.
    /// </summary>
    public string UserId { get; } = userId;

    /// <summary>
    /// Gets the optional email address of the user.
    /// </summary>
    public string? UserEmail { get; } = userEmail;

    /// <summary>
    /// Gets the set of roles assigned to the user for authorization scenarios.
    /// </summary>
    public string[] Roles { get; } = roles ?? [];

    /// <summary>
    /// Gets the tenant identifier associated with this execution scope.
    /// </summary>
    public string TenantId { get; } = tenantId;

    /// <summary>
    /// Gets the optional organization identifier, if multi-org classification is active.
    /// </summary>
    public string? OrganizationId { get; } = organizationId;

    /// <summary>
    /// Correlation identifier used to link requests across distributed components.
    /// </summary>
    public string CorrelationId { get; } = correlationId;

    /// <summary>
    /// Distributed tracing identifier representing a trace.
    /// </summary>
    public string? TraceId { get; } = traceId;

    /// <summary>
    /// Distributed tracing identifier representing a span within a trace.
    /// </summary>
    public string? SpanId { get; } = spanId;

    /// <summary>
    /// Arbitrary metadata attached to this context instance. 
    /// Trusted components may enrich or modify these values.
    /// </summary>
    public Dictionary<string, object> Metadata { get; } = new();

    /// <summary>
    /// Set of enabled feature flags associated with this execution context.
    /// </summary>
    public HashSet<string> EnabledFeatures { get; } = [];

    /// <summary>
    /// Executes a sensitive operation allowed only for trusted subsystems.
    /// Mutates internal metadata.
    /// </summary>
    public void DoSomething()
    {
        Console.WriteLine($"[MyContext] DoSomething called by {UserId}");

        Metadata["LastAction"] = "DoSomething";
        Metadata["ActionTimestamp"] = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Mutates the tenant assignment for this context.
    /// </summary>
    /// <param name="newTenantId">The new tenant identifier to assign.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Represents a privileged operation restricted to trusted code paths.</item>
    /// <item>A production-grade implementation SHOULD treat context as immutable and produce a new instance.</item>
    /// </list>
    /// </remarks>
    public void ModifyTenant(string newTenantId)
    {
        Console.WriteLine($"[MyContext] Changing tenant from {TenantId} to {newTenantId}");
        // Mutation demonstration only
    }

    /// <summary>
    /// Determines whether the user possesses a specific role.
    /// </summary>
    /// <param name="role">The role to verify.</param>
    /// <returns>True if the user holds the specified role; otherwise false.</returns>
    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether a named feature flag is enabled for this context.
    /// </summary>
    /// <param name="feature">The feature name.</param>
    /// <returns>True if the feature is enabled; otherwise false.</returns>
    public bool IsFeatureEnabled(string feature) =>
        EnabledFeatures.Contains(feature);

    /// <summary>
    /// Creates a new <see cref="MyContext"/> instance with default tracing and ID generation.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userEmail">Optional email address.</param>
    /// <param name="roles">Optional role set.</param>
    /// <returns>A new fully initialized <see cref="MyContext"/> instance.</returns>
    public static MyContext Create(
        string userId,
        string tenantId,
        string? userEmail = null,
        string[]? roles = null)
    {
        return new MyContext(
            id: Guid.NewGuid().ToString("N"),
            userId: userId,
            tenantId: tenantId,
            correlationId: Ulid.NewUlid().ToString(),
            userEmail: userEmail,
            roles: roles
        );
    }

    /// <summary>
    /// Creates a new <see cref="MyContext"/> with explicitly provided distributed tracing identifiers.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="traceId">The trace identifier.</param>
    /// <param name="spanId">The span identifier.</param>
    /// <param name="userEmail">Optional email address.</param>
    /// <param name="roles">Optional role set.</param>
    /// <returns>A fully initialized <see cref="MyContext"/> instance with tracing attached.</returns>
    public static MyContext CreateWithTracing(
        string userId,
        string tenantId,
        string traceId,
        string spanId,
        string? userEmail = null,
        string[]? roles = null)
    {
        return new MyContext(
            id: Guid.NewGuid().ToString("N"),
            userId: userId,
            tenantId: tenantId,
            correlationId: Ulid.NewUlid().ToString(),
            traceId: traceId,
            spanId: spanId,
            userEmail: userEmail,
            roles: roles
        );
    }

    /// <summary>
    /// Creates a minimal context instance with default tenant and generated identifiers.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A minimal <see cref="MyContext"/> suitable for testing or lightweight flows.</returns>
    public static MyContext CreateMinimal(string userId) =>
        new(
            id: Guid.NewGuid().ToString("N"),
            userId: userId,
            tenantId: "default",
            correlationId: Ulid.NewUlid().ToString()
        );
}
