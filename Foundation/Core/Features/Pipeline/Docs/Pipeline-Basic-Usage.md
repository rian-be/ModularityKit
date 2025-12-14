# Pipeline Basic Usage

A step-by-step guide on using the Pipeline System for building composable, conditional, and branch able middleware pipelines.

---

## ⚡ Quick Start
```csharp
using Core.Features.Pipeline.Runtime;
using Core.Features.Pipeline.Abstractions.Middleware;

// Define context
public sealed class RequestContext
{
    public string UserId { get; init; }
    public string TenantId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; }
}

// Create middleware
public class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Start {context.UserId}");
        await next();
        Console.WriteLine($"End {context.UserId}");
    }
}

public class ValidationMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        if (!context.Roles.Contains("User"))
            throw new InvalidOperationException("User role required");
        await next();
    }
}
```

---

## Middleware Registration

### 1. Use – Standard Middleware

Adds a middleware that always executes.
```csharp
var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
```

**Execution Flow:**
```
Request → LoggingMiddleware → ValidationMiddleware → Response
```

---

### 2. UseWhen – Conditional Middleware

Executes middleware only if a predicate matches.
```csharp
builder.Use(new LoggingMiddleware());
builder.UseWhen(ctx => ctx.Roles.Contains("Admin"), new AdminAuditMiddleware());
builder.Use(new ProcessingMiddleware());
```

**Execution Flow:**
```
All Users:
    LoggingMiddleware → ProcessingMiddleware

Admin Users:
    LoggingMiddleware → AdminAuditMiddleware → ProcessingMiddleware
```

**Example:**
```csharp
var builder = new PipelineBuilder<RequestContext>();

// Always execute
builder.Use(new LoggingMiddleware());

// Only for admins
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);

// Only for premium tenants
builder.UseWhen(
    ctx => ctx.TenantId.StartsWith("premium-"),
    new PremiumFeaturesMiddleware()
);

// Always execute
builder.Use(new ProcessingMiddleware());
```

---

### 3. UseBranch – Branch Pipelines

Creates isolated sub-pipelines for specific conditions.
```csharp
builder.Use(new LoggingMiddleware());

builder.UseBranch(
    condition: ctx => ctx.TenantId == "tenant-456",
    configurePipeline: branch =>
    {
        branch.Use(new AuditMiddleware());
        branch.Use(new FeatureToggleMiddleware());
    }
);

builder.Use(new FinalMiddleware());
```

**Execution Flow:**
```
All Tenants:
    LoggingMiddleware → FinalMiddleware

Tenant 456:
    LoggingMiddleware → AuditMiddleware → FeatureToggleMiddleware → FinalMiddleware
```

**Example:**
```csharp
var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());

// Branch for free tier
builder.UseBranch(
    ctx => ctx.Metadata["Plan"] as string == "Free",
    freeBranch =>
    {
        freeBranch.Use(new RateLimitingMiddleware());
        freeBranch.Use(new BasicFeaturesMiddleware());
    }
);

// Branch for premium tier
builder.UseBranch(
    ctx => ctx.Metadata["Plan"] as string == "Premium",
    premiumBranch =>
    {
        premiumBranch.Use(new PremiumFeaturesMiddleware());
        premiumBranch.Use(new AnalyticsMiddleware());
    }
);

builder.Use(new CommonProcessingMiddleware());
```

---

## Executing the Pipeline
```csharp
var executor = new PipelineExecutor<RequestContext>(builder);

var context = new RequestContext
{
    UserId = "user-123",
    TenantId = "tenant-456",
    Roles = new[] { "User", "Admin" }
};

await executor.ExecuteAsync(context);
```

**With Cancellation Token:**
```csharp
var cts = new CancellationTokenSource();

try
{
    await executor.ExecuteAsync(context, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Pipeline execution was cancelled");
}
```

---

## Inspecting Middleware Execution

Optionally wrap the pipeline with debug instrumentation:
```csharp
using Core.Features.Pipeline.Diagnostics;

using var scope = PipelineDebugScope.Begin(out var debug);
await executor.ExecuteAsync(context);

// Inspect steps
foreach (var step in debug.Steps)
{
    Console.WriteLine(
        $"{step.Middleware.GetType().Name} | " +
        $"{step.Duration?.TotalMilliseconds:F3}ms | " +
        $"Next={step.NextCalled}"
    );
}
```

**Output Example:**
```
LoggingMiddleware | 2.345ms | Next=True
ValidationMiddleware | 0.123ms | Next=True
AuditMiddleware | 1.567ms | Next=True
FeatureToggleMiddleware | 0.456ms | Next=True
```

**Performance Analysis:**
```csharp
using var scope = PipelineDebugScope.Begin(out var debug);
await executor.ExecuteAsync(context);

// Find slowest middleware
var slowest = debug.Steps
    .OrderByDescending(s => s.Duration)
    .First();

Console.WriteLine(
    $"Slowest: {slowest.Middleware.GetType().Name} " +
    $"({slowest.Duration?.TotalMilliseconds:F3}ms)"
);

// Total execution time
var totalMs = debug.Steps.Sum(s => s.Duration?.TotalMilliseconds ?? 0);
Console.WriteLine($"Total: {totalMs:F3}ms");
```

---

## Common Patterns

### Pattern 1: Authentication & Authorization Pipeline
```csharp
var builder = new PipelineBuilder<ApiContext>();

// Always execute
builder.Use(new LoggingMiddleware());
builder.Use(new ExceptionHandlingMiddleware());

// Authentication
builder.Use(new AuthenticationMiddleware());

// Authorization - only for authenticated users
builder.UseWhen(
    ctx => ctx.IsAuthenticated,
    new AuthorizationMiddleware()
);

// Process request
builder.Use(new RequestProcessingMiddleware());
```

### Pattern 2: Multi-Tenant Feature Flags
```csharp
var builder = new PipelineBuilder<TenantContext>();

builder.Use(new LoggingMiddleware());

// Feature branch for beta features
builder.UseBranch(
    ctx => ctx.Features.Contains("BetaAccess"),
    betaBranch =>
    {
        betaBranch.Use(new BetaFeaturesMiddleware());
        betaBranch.Use(new BetaAnalyticsMiddleware());
    }
);

// Feature branch for enterprise features
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);

builder.Use(new StandardProcessingMiddleware());
```

### Pattern 3: Rate Limiting by User Tier
```csharp
var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());

// Free tier - strict rate limiting
builder.UseWhen(
    ctx => ctx.Metadata["Tier"] as string == "Free",
    new RateLimitingMiddleware(maxRequests: 100, window: TimeSpan.FromHours(1))
);

// Pro tier - relaxed rate limiting
builder.UseWhen(
    ctx => ctx.Metadata["Tier"] as string == "Pro",
    new RateLimitingMiddleware(maxRequests: 1000, window: TimeSpan.FromHours(1))
);

// Enterprise tier - no rate limiting
builder.Use(new ProcessingMiddleware());
```
---

## 🔗 Next Steps

- **[Pipeline Guide](Pipeline-Guide.md)** - Complete pipeline development guide
- **[Conditional Middleware](Pipeline-Guide.md#conditional-middleware)** - Advanced conditional patterns
- **[Branch Pipelines](Pipeline-Guide.md#branch-pipelines)** - Complex routing scenarios
- **[Runtime Inspection](Pipeline-Guide.md#runtime-inspection)** - Dynamic modifications
- **[Diagnostics & Debugging](Pipeline-Guide.md#pipeline-diagnostics)** - Performance profiling

---

> **Built with ❤️ for .NET developers**