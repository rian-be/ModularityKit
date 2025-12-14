# Pipeline System

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-purple.svg)](https://dotnet.microsoft.com/)

A high-performance, composable middleware pipeline system with built-in diagnostics and runtime inspection.

## 🚀 Features

- **Composable Middleware** - Clear, ordered execution of middleware components
- **Async-First Design** - Fully async, allocation-aware execution model
- **Conditional Execution** - `UseWhen` for context-based middleware activation
- **Branch Pipelines** - `UseBranch` for isolated sub-pipelines
- **Runtime Inspection** - Inspect, remove, or modify middleware at runtime
- **Zero-Cost Diagnostics** - Optional debug instrumentation with no overhead when disabled
- **High Performance** - No reflection, no dynamic dispatch in hot path

---

## ⚡ Quick Start
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
builder.UseWhen(ctx => ctx.Roles.Contains("User"), new ValidationMiddleware());
builder.UseBranch(
    ctx => ctx.TenantId == "tenant-456",
    branch =>
    {
        branch.Use(new AuditMiddleware());
        branch.Use(new FeatureToggleMiddleware());
    }
);

// 4. Execute
var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(new RequestContext
{
    UserId = "user-123",
    TenantId = "tenant-456",
    Roles = new[] { "User" }
});
```

---

## Why Use Pipeline System?

### Without Pipeline ❌
```csharp
public async Task Handle(RequestContext ctx)
{
    await Log(ctx);
    
    if (ctx.Roles.Contains("User"))
        await Validate(ctx);
        
    if (ctx.TenantId == "tenant-456")
    {
        await Audit(ctx);
        await FeatureToggle(ctx);
    }
}
```

### With Pipeline ✅
```csharp
builder.Use(new LoggingMiddleware());
builder.UseWhen(ctx => ctx.Roles.Contains("User"), new ValidationMiddleware());
builder.UseBranch(
    ctx => ctx.TenantId == "tenant-456",
    branch =>
    {
        branch.Use(new AuditMiddleware());
        branch.Use(new FeatureToggleMiddleware());
    }
);
```

**Declarative, testable, reorderable, inspectable.**

---

## Documentation

### Getting Started
- **[Installation & Setup](Docs/Pipeline-Getting-Started.md)** - Complete setup guide
- **[Basic Usage](Docs/Pipeline-Basic-Usage.md)** - Learn the fundamentals
- **[Core Concepts](Docs/Pipeline-Concepts.md)** - Understanding the architecture

### Guides
- **[Pipeline Guide](Docs/Pipeline-Guide.md)** - Complete pipeline development guide
- **[Conditional Middleware](Docs/Pipeline-Guide.md#conditional-middleware)** - Context-based execution
- **[Branch Pipelines](Docs/Pipeline-Guide.md#branch-pipelines)** - Isolated sub-pipelines
- **[Diagnostics & Debugging](Docs/Pipeline-Guide.md#pipeline-diagnostics)** - Performance analysis
- **[Runtime Inspection](Docs/Pipeline-Guide.md#runtime-inspection)** - Dynamic pipeline manipulation

### Reference
- **[API Reference](Docs/Pipeline-API-Reference.md)** - Complete API documentation
- **[Best Practices](Docs/Pipeline-Best-Practices.md)** - Tips and recommendations

---

## Common Use Cases

### Web API Request Pipeline
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

[**See full example →**](Docs/Pipeline-Guide.md#example-1-web-api-request-pipeline)

### Multi-Tenant Pipeline
```csharp
var builder = new PipelineBuilder<TenantContext>();

builder.Use(new LoggingMiddleware());
builder.Use(new TenantResolutionMiddleware());

// Branch by tenant plan
builder.UseBranch(
    ctx => ctx.Plan == "Free",
    freeBranch =>
    {
        freeBranch.Use(new RateLimitingMiddleware(limit: 100));
        freeBranch.Use(new FeatureLimitMiddleware());
    }
);

builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);

builder.Use(new ProcessingMiddleware());
```

[**See full example →**](Docs/Pipeline-Guide.md#example-2-multi-tenant-pipeline)

---

## Pipeline Diagnostics

Optional debug layer with zero runtime overhead when disabled.
```csharp
using Core.Features.Pipeline.Diagnostics;

var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());

var executor = new PipelineExecutor<RequestContext>(builder);

// Enable diagnostics
using var scope = PipelineDebugScope.Begin(out var debug);

await executor.ExecuteAsync(context);

// Inspect execution
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

**What Gets Captured:**

- Middleware identity
- Execution duration
- Whether `next()` was called
- Execution order

[**Learn more about diagnostics →**](Docs/Pipeline-Guide.md#pipeline-diagnostics)

---

## Runtime Inspection

Inspect and manipulate pipeline structure at runtime.
```csharp
using Core.Features.Pipeline.Inspection;

var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());

var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// List middleware
foreach (var mw in inspector.GetMiddlewares())
{
    Console.WriteLine(mw.GetType().Name);
}

// Remove middleware dynamically
inspector.Remove(mw => mw is ValidationMiddleware);

// Get metadata
foreach (var descriptor in inspector.GetDescriptors())
{
    Console.WriteLine(
        $"{descriptor.Name} [{descriptor.Kind}] " +
        $"Conditional={descriptor.IsConditional}"
    );
}
```

**Descriptors are resolved via:**

- Attributes
- Metadata interfaces
- Descriptor providers
- Cached resolution (thread-safe)

[**Learn more about inspection →**](Docs/Pipeline-Guide.md#runtime-inspection)

---

## Architecture
```
Application Layer
    │
    ▼
PipelineBuilder<TContext>
    ├─ Use
    ├─ UseWhen
    ├─ UseBranch
    │
    ▼
PipelineExecutor<TContext>
    ├─ Sequential execution
    ├─ Async continuation
    └─ Optional Debug Wrapper
    │
    ▼
Middleware Chain
    ├─ Standard Middleware
    ├─ Conditional Middleware
    └─ Branch Middleware
```
[**Learn more about architecture →**](Docs/Architecture.md)
---

## Execution Model

- ✅ No reflection in hot path
- ✅ No dynamic dispatch
- ✅ Single delegate allocation per middleware
- ✅ AsyncLocal only when debugging is enabled
- ✅ Middleware controls continuation (`next`)

---

## Best Practices

### DO ✅

- Keep middleware small and single-purpose
- Prefer `UseWhen` over internal `if` logic
- Use diagnostics only in development
- Treat middleware as stateless

### DON'T ❌

- Perform I/O in diagnostic middleware
- Use `Console.WriteLine` inside middleware
- Mutate pipeline during execution
- Share middleware instances with state

[**Read full best practices guide →**](Docs/Pipeline-Best-Practices.md)

---

## API Reference

### Core Types

- **[PipelineBuilder\<TContext\>](Docs/Pipeline-API-Reference.md#pipelinebuildertcontext)** - Pipeline composition
- **[PipelineExecutor\<TContext\>](Docs/Pipeline-API-Reference.md#pipelineexecutortcontext)** - Execution engine
- **[IMiddleware\<TContext\>](Docs/Pipeline-API-Reference.md#imiddlewaretcontext)** - Middleware contract

### Diagnostics

- **[PipelineDebugScope](Docs/Pipeline-API-Reference.md#pipelinedebugscope)** - Debug scope management
- **[PipelineDebugContext](Docs/Pipeline-API-Reference.md#pipelinedebugcontext)** - Debug information container
- **[PipelineDebugStep](Docs/Pipeline-API-Reference.md#pipelinedebugstep)** - Step-level diagnostics
- **[PipelineDebuggerMiddleware](Docs/Pipeline-API-Reference.md#pipelinedebuggermiddleware)** - Debug wrapper

### Inspection

- **[PipelineInspector\<TContext\>](Docs/Pipeline-API-Reference.md#pipelineinspectortcontext)** - Runtime inspection
- **[IMiddlewareDescriptor](Docs/Pipeline-API-Reference.md#imiddlewaredescriptor)** - Middleware metadata
- **[MiddlewareDescriptorProvider](Docs/Pipeline-API-Reference.md#middlewaredescriptorprovider)** - Metadata resolution

[**View complete API reference →**](Docs/Pipeline-API-Reference.md)

---

> **Built with ❤️ for .NET developers**