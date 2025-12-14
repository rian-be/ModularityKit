# Pipeline Guide

Complete guide for building composable, high-performance middleware pipelines.

---

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Basic Pipeline Composition](#basic-pipeline-composition)
- [Conditional Middleware](#conditional-middleware)
- [Branch Pipelines](#branch-pipelines)
- [Pipeline Diagnostics](#pipeline-diagnostics)
- [Runtime Inspection](#runtime-inspection)
- [Advanced Use Cases](#advanced-use-cases)
- [Performance Notes](#performance-notes)

---

## Overview

This guide demonstrates how to effectively design, build, and debug pipelines using the Pipeline System. It covers conditional middleware, branch pipelines, diagnostics, and runtime inspection.

---

## Quick Start
```csharp
using Core.Features.Pipeline.Runtime;
using Core.Features.Pipeline.Abstractions.Middleware;

// 1. Define context
public sealed class RequestContext
{
    public string UserId { get; init; }
    public string TenantId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; }
}

// 2. Create middleware
public class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Request started for user: {context.UserId}");
        await next();
        Console.WriteLine($"Request completed");
    }
}

// 3. Build pipeline
var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());

// 4. Execute
var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(context);
```

---

## Basic Pipeline Composition

Use `PipelineBuilder<TContext>` to declare middleware execution order:
```csharp
var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());

var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(context);
```

**Key Points:**

- Middleware executes in registration order
- Each middleware receives a continuation delegate `next()`
- Skipping `next()` short-circuits the pipeline

**Execution Flow:**
```
Request → LoggingMiddleware → ValidationMiddleware → ProcessingMiddleware → Response
```

---

## Conditional Middleware

Conditional middlewares execute only when a predicate matches:
```csharp
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

builder.Use(new ProcessingMiddleware());
```

**Benefits:**

- ✅ No manual `if` checks inside middleware
- ✅ Declarative, testable, and reorderable
- ✅ Can be combined with branches for complex scenarios

**Execution Flow:**
```
All Users:
    Logging → Processing

Admin Users:
    Logging → AdminAudit → Processing

Premium Tenant:
    Logging → PremiumFeatures → Processing
```

### Multiple Conditions
```csharp
// Complex condition
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin") && ctx.TenantId.StartsWith("enterprise-"),
    new EnterpriseAdminMiddleware()
);

// Condition with metadata
builder.UseWhen(
    ctx => ctx.Metadata.TryGetValue("DebugMode", out var debug) && (bool)debug,
    new DebugMiddleware()
);
```

---

## Branch Pipelines

Branch pipelines allow isolated sub-pipelines:
```csharp
builder.Use(new LoggingMiddleware());

builder.UseBranch(
    ctx => ctx.TenantId == "tenant-456",
    branch =>
    {
        branch.Use(new AuditMiddleware());
        branch.Use(new FeatureToggleMiddleware());
    }
);

builder.Use(new FinalMiddleware());
```

**Characteristics:**

- ✅ Branch executes only if predicate matches
- ✅ Middlewares inside the branch do not affect main pipeline ordering
- ✅ Nested branches are supported

**Execution Flow:**
```
All Tenants:
    Logging → FinalMiddleware

Tenant 456:
    Logging → Audit → FeatureToggle → FinalMiddleware
```

### Nested Branches
```csharp
builder.Use(new LoggingMiddleware());

builder.UseBranch(
    ctx => ctx.Roles.Contains("Admin"),
    adminBranch =>
    {
        adminBranch.Use(new AdminAuthMiddleware());
        
        // Nested branch for enterprise admins
        adminBranch.UseBranch(
            ctx => ctx.TenantId.StartsWith("enterprise-"),
            enterpriseBranch =>
            {
                enterpriseBranch.Use(new EnterpriseAuditMiddleware());
                enterpriseBranch.Use(new EnterpriseReportingMiddleware());
            }
        );
        
        adminBranch.Use(new AdminProcessingMiddleware());
    }
);
```

### Multiple Branches
```csharp
var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());

// Branch A: Free tier
builder.UseBranch(
    ctx => ctx.Metadata["Plan"] as string == "Free",
    freeBranch =>
    {
        freeBranch.Use(new RateLimitingMiddleware());
        freeBranch.Use(new FreeFeatureMiddleware());
    }
);

// Branch B: Premium tier
builder.UseBranch(
    ctx => ctx.Metadata["Plan"] as string == "Premium",
    premiumBranch =>
    {
        premiumBranch.Use(new PremiumFeaturesMiddleware());
        premiumBranch.Use(new PriorityProcessingMiddleware());
    }
);

// Branch C: Enterprise tier
builder.UseBranch(
    ctx => ctx.Metadata["Plan"] as string == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);

builder.Use(new CommonProcessingMiddleware());
```

---

## Pipeline Diagnostics

Use `PipelineDebugScope` to capture per-middleware diagnostics:
```csharp
using Core.Features.Pipeline.Diagnostics;

using var scope = PipelineDebugScope.Begin(out var debug);
await executor.ExecuteAsync(context);

foreach (var step in debug.Steps)
{
    Console.WriteLine(
        $"{step.Middleware.GetType().Name} | " +
        $"{step.Duration?.TotalMilliseconds:F3}ms | " +
        $"Next={step.NextCalled}"
    );
}
```

**Output:**
```
LoggingMiddleware | 12.345ms | Next=True
ValidationMiddleware | 5.123ms | Next=True
ProcessingMiddleware | 45.678ms | Next=False
```

### Captured Data

| Property     | Type                    | Description                         |
|--------------|-------------------------|-------------------------------------|
| `Middleware` | `IMiddleware<TContext>` | The middleware instance             |
| `Duration`   | `TimeSpan?`             | Execution time (including `next()`) |
| `NextCalled` | `bool`                  | Whether `next()` was invoked        |
| `Order`      | `int`                   | Execution order in pipeline         |

### Performance Analysis
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

// Find middleware that didn't call next
var terminal = debug.Steps
    .Where(s => !s.NextCalled)
    .ToList();

Console.WriteLine($"Terminal middleware: {terminal.Count}");

// Total execution time
var totalMs = debug.Steps.Sum(s => s.Duration?.TotalMilliseconds ?? 0);
Console.WriteLine($"Total: {totalMs:F3}ms");
```

### Conditional Diagnostics
```csharp
var shouldDebug = configuration.GetValue<bool>("EnablePipelineDebugging");

if (shouldDebug)
{
    using var scope = PipelineDebugScope.Begin(out var debug);
    await executor.ExecuteAsync(context);
    
    // Log diagnostics
    logger.LogInformation(
        "Pipeline executed in {Duration}ms",
        debug.Steps.Sum(s => s.Duration?.TotalMilliseconds ?? 0)
    );
}
else
{
    // Zero overhead
    await executor.ExecuteAsync(context);
}
```

**Notes:**

- ✅ Zero-cost in production when disabled
- ✅ Safe for async flows and exceptions
- ✅ Thread-safe and async-safe

---

## Runtime Inspection

Inspect or modify pipeline structure dynamically using `PipelineInspector<TContext>`:
```csharp
using Core.Features.Pipeline.Inspection;

var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());

var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// List all middleware
Console.WriteLine("Middleware in pipeline:");
foreach (var mw in inspector.GetMiddlewares())
{
    Console.WriteLine($"  - {mw.GetType().Name}");
}

// Remove a specific middleware
inspector.Remove(mw => mw is ValidationMiddleware);

// Inspect metadata descriptors
foreach (var desc in inspector.GetDescriptors())
{
    Console.WriteLine(
        $"{desc.Name} [{desc.Kind}] " +
        $"Conditional={desc.IsConditional}"
    );
}
```

**Output:**
```
Middleware in pipeline:
  - LoggingMiddleware
  - ValidationMiddleware
  - ProcessingMiddleware

LoggingMiddleware [Standard] Conditional=False
ProcessingMiddleware [Standard] Conditional=False
```

### Middleware Descriptors

| Property        | Type             | Description                        |
|-----------------|------------------|------------------------------------|
| `Name`          | `string`         | Middleware name                    |
| `Kind`          | `MiddlewareKind` | Standard, Conditional, Branch      |
| `IsConditional` | `bool`           | Whether middleware has a condition |
| `Description`   | `string?`        | Optional description               |

### Features

- ✅ Middleware metadata resolved via attributes and provider
- ✅ Thread-safe inspection
- ✅ Enables dynamic pipeline adjustments at runtime

### Remove Middleware
```csharp
var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// Remove specific middleware type
inspector.Remove(mw => mw is ValidationMiddleware);

// Remove by condition
inspector.Remove(mw => mw.GetType().Name.Contains("Debug"));

// Rebuild executor
var executor = new PipelineExecutor<RequestContext>(builder);
```

### Modify Pipeline
```csharp
// Get current middleware
var middlewares = inspector.GetMiddlewares().ToList();

// Add new middleware before validation
var validationIndex = middlewares.FindIndex(m => m is ValidationMiddleware);
if (validationIndex >= 0)
{
    builder.Middlewares.Insert(validationIndex, new AuthenticationMiddleware());
}

// Rebuild executor
var executor = new PipelineExecutor<RequestContext>(builder);
```

---

## Advanced Use Cases

### Example 1: Web API Request Pipeline
```csharp
var builder = new PipelineBuilder<ApiRequestContext>();

// Always execute
builder.Use(new RequestLoggingMiddleware());
builder.Use(new ExceptionHandlingMiddleware());
builder.Use(new AuthenticationMiddleware());
builder.Use(new AuthorizationMiddleware());

// Conditional execution
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);

// Process request
builder.Use(new ValidationMiddleware());
builder.Use(new RateLimitingMiddleware());
builder.Use(new RequestProcessingMiddleware());

var executor = new PipelineExecutor<ApiRequestContext>(builder);
```

**Benefits:**

- Clear separation of concerns
- Declarative middleware ordering
- Conditional execution based on roles
- Easy to test and modify

### Example 2: Multi-Tenant Pipeline
```csharp
var builder = new PipelineBuilder<TenantContext>();

builder.Use(new LoggingMiddleware());
builder.Use(new TenantResolutionMiddleware());

// Free tier
builder.UseBranch(
    ctx => ctx.Plan == "Free",
    freeBranch =>
    {
        freeBranch.Use(new RateLimitingMiddleware(limit: 100));
        freeBranch.Use(new FeatureLimitMiddleware());
    }
);

// Premium tier
builder.UseBranch(
    ctx => ctx.Plan == "Premium",
    premiumBranch =>
    {
        premiumBranch.Use(new RateLimitingMiddleware(limit: 1000));
        premiumBranch.Use(new PremiumFeaturesMiddleware());
        premiumBranch.Use(new AnalyticsMiddleware());
    }
);

// Enterprise tier
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new NoRateLimitMiddleware());
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);

builder.Use(new ProcessingMiddleware());
```

**Benefits:**

- ✅ Declarative branch handling
- ✅ Conditional execution based on tenant plan
- ✅ Clear separation of concerns

---

## Performance Notes

The Pipeline System is designed for high performance:

- ✅ **No reflection in hot path** - All middleware invocation uses direct delegates
- ✅ **Single delegate allocation per middleware** - Minimal memory overhead
- ✅ **AsyncLocal only when debugging** - Zero cost when diagnostics are disabled
- ✅ **Optimized branches** - Branches do not allocate unless executed

**Performance Goal:** ~100ns overhead per middleware when debugging is disabled.

---

## See Also

- [Getting Started](Pipeline-Getting-Started.md) - Setup and basic usage
- [Basic Usage](Pipeline-Basic-Usage.md) - Common patterns
- [Core Concepts](Pipeline-Core-Concepts.md) - Understanding the architecture
- [API Reference](Pipeline-API-Reference.md) - Complete API documentation
- [Best Practices](Pipeline-Best-Practices.md) - Tips and recommendations

---

> **Built with ❤️ for .NET developers**