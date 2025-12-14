# Pipeline Core Concepts

The Pipeline System provides a structured, composable, and async-first middleware execution model. This document explains the internal architecture, execution model, and design goals.

---

## Overview

A **Pipeline** represents a chain of middleware components that handle a unit of work. Each middleware can:

- Inspect or modify the context
- Decide whether to invoke the next middleware (`next()`)
- Branch execution or conditionally execute based on the context
- Be instrumented for diagnostics without affecting runtime performance

Pipelines are **not global**. Each pipeline instance operates independently and can be inspected or modified at runtime through dedicated APIs.

---

## Goals of the Pipeline System

The design is guided by four core objectives:

### 1. Composable & Declarative

- Middlewares are registered in order via `PipelineBuilder<TContext>`
- Branches and conditional middlewares are first-class primitives

### 2. Async-Safe Execution

- Continuations are fully async-aware and allocation-minimal
- `AsyncLocal` is only used when diagnostics are enabled

### 3. Observability

- Optional debug instrumentation captures per-middleware timing, order, and next-call status

### 4. Runtime Modifiability

- Pipelines can be inspected, cleared, or modified dynamically using `PipelineInspector<TContext>`

---

## PipelineBuilder\<TContext\>

`PipelineBuilder<TContext>` is responsible for declaratively composing the middleware chain.

### Key Operations

| Method        | Behavior                                                    |
|---------------|-------------------------------------------------------------|
| `Use()`       | Always executes middleware in order                         |
| `UseWhen()`   | Executes middleware only if a predicate matches             |
| `UseBranch()` | Executes an isolated branch pipeline if a condition matches |

The builder produces a sequential list of `IMiddleware<TContext>` ready for execution by `PipelineExecutor<TContext>`.

### Example
```csharp
var builder = new PipelineBuilder<RequestContext>();

// Always execute
builder.Use(new LoggingMiddleware());

// Conditional execution
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);

// Branch execution
builder.UseBranch(
    ctx => ctx.TenantId == "premium",
    branch =>
    {
        branch.Use(new PremiumFeaturesMiddleware());
        branch.Use(new AnalyticsMiddleware());
    }
);
```

---

## PipelineExecutor\<TContext\>

`PipelineExecutor<TContext>` executes the registered middlewares sequentially.

### Responsibilities

- Invoke each middleware with the current context
- Provide async continuation through `Func<Task> next`
- Wrap middlewares with `PipelineDebuggerMiddleware` when diagnostics are enabled
- Handle branch pipelines and conditional execution transparently

### Example
```csharp
await executor.ExecuteAsync(context);
```

**Important:** A middleware is fully responsible for calling `next()`; skipping it short-circuits the pipeline.

---

## Middleware Types

| Type                       | Description                                            |
|----------------------------|--------------------------------------------------------|
| **Standard Middleware**    | Always executed in order                               |
| **Conditional Middleware** | Executes only if predicate matches                     |
| **Branch Middleware**      | Executes an isolated sub-pipeline if condition matches |

### Standard Middleware
```csharp
public class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Before");
        await next();
        Console.WriteLine("After");
    }
}
```

### Conditional Middleware
```csharp
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);
```

**Execution:**
- Predicate is evaluated for each request
- Middleware only executes if condition is `true`

### Branch Middleware
```csharp
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);
```

**Execution:**
- Condition is evaluated for each request
- If `true`, entire branch pipeline executes
- Branch is isolated from main pipeline

---

## Debug Instrumentation

Optional diagnostics allow **zero-cost instrumentation** in production.

### What Gets Captured

Per-middleware:
- Start time
- Duration
- Whether `next()` was called
- Execution order

Exposed through `PipelineDebugScope` and `PipelineDebugContext.Steps`.

### Example
```csharp
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

**Diagnostics are safe to use in development without affecting production performance.**

---

## PipelineInspector\<TContext\>

Allows runtime inspection and manipulation of pipeline structure.

### Capabilities

- Enumerate all middlewares
- Remove middlewares dynamically
- Clear the pipeline
- Retrieve metadata via `IMiddlewareDescriptor` and `MiddlewareDescriptorProvider`

### Example
```csharp
var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// Remove a middleware
inspector.Remove(mw => mw is ValidationMiddleware);

// Inspect descriptors
foreach (var desc in inspector.GetDescriptors())
{
    Console.WriteLine(
        $"{desc.Name} [{desc.Kind}] " +
        $"Conditional={desc.IsConditional}"
    );
}
```

This enables **dynamic pipeline modification** without recompilation.

---

## Execution Model

The Pipeline System uses a highly optimized execution model:

- ✅ Single delegate allocation per middleware
- ✅ No reflection in hot path
- ✅ Async-first, continuation-based execution
- ✅ Branches executed only when conditions are met
- ✅ Debug instrumentation uses `AsyncLocal` only when enabled

**Performance Goal:** Minimal overhead (~100ns per middleware) when debugging is disabled.

### Execution Flow
```
Request
    │
    ▼
┌─────────────────────┐
│ Middleware 1        │
│ ┌─────────────────┐ │
│ │ Before next()   │ │
│ │     │           │ │
│ │     ▼           │ │
│ │ Middleware 2    │ │
│ │ ┌─────────────┐ │ │
│ │ │ Before next │ │ │
│ │ │     │       │ │ │
│ │ │     ▼       │ │ │
│ │ │ Middleware 3│ │ │
│ │ │ (Terminal)  │ │ │
│ │ │     ▲       │ │ │
│ │ │     │       │ │ │
│ │ │ After next  │ │ │
│ │ └─────────────┘ │ │
│ │     │           │ │
│ │     ▼           │ │
│ │ After next()    │ │
│ └─────────────────┘ │
│     │               │
│     ▼               │
│ After next()        │
└─────────────────────┘
    │
    ▼
Response
```

---

## Pipeline Lifecycle

A middleware pipeline follows this strict lifecycle:
```
[BUILD] → [EXECUTE] → [INSPECT/DEBUG] → [CLEANUP]
```

### 1. Build

Using `PipelineBuilder<TContext>` to register middlewares.
```csharp
var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
```

### 2. Execute

Sequential invocation via `PipelineExecutor<TContext>`.
```csharp
var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(context);
```

### 3. Inspect/Debug

Optional instrumentation and runtime inspection.
```csharp
using var scope = PipelineDebugScope.Begin(out var debug);
await executor.ExecuteAsync(context);
// Inspect debug.Steps
```

### 4. Cleanup

Pipeline completes, debug scope ends, `AsyncLocal` cleared.

---

## Error Handling

### Exception Propagation

- Exceptions in middleware propagate naturally
- Debug instrumentation still records execution duration even if `next()` fails
- No state leakage between pipeline executions

### Example
```csharp
public class ExceptionHandlingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw; // Re-throw or handle
        }
    }
}
```

---

## Threading and Async Behavior

### Key Characteristics

- Middleware binds to the **logical async flow**, not the physical thread
- Branch pipelines respect async continuation
- Parallel tasks do not interfere with each other's pipeline execution
- Cleanup ensures no state leakage between executions

### Example: Concurrent Execution
```csharp
var executor = new PipelineExecutor<RequestContext>(builder);

var tasks = Enumerable.Range(1, 5).Select(i => Task.Run(async () =>
{
    var context = new RequestContext { UserId = $"user-{i}" };
    await executor.ExecuteAsync(context);
}));

await Task.WhenAll(tasks);
// Each task has isolated pipeline execution ✅
```

---

## Architecture Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│                                                             │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐ │
│  │   Context    │    │  Middleware  │    │   Executor   │ │
│  └──────────────┘    └──────────────┘    └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                 Pipeline Infrastructure                      │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            PipelineBuilder<TContext>                  │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────────┐   │  │
│  │  │   Use    │  │ UseWhen  │  │   UseBranch      │   │  │
│  │  └──────────┘  └──────────┘  └──────────────────┘   │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                │
│                            ▼                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          PipelineExecutor<TContext>                   │  │
│  │  ┌──────────────────────────────────────────────┐    │  │
│  │  │  Sequential Middleware Invocation            │    │  │
│  │  │  ┌────────────────────────────────────────┐  │    │  │
│  │  │  │  Optional PipelineDebuggerMiddleware   │  │    │  │
│  │  │  └────────────────────────────────────────┘  │    │  │
│  │  └──────────────────────────────────────────────┘    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Diagnostics & Inspection                    │
│                                                             │
│  ┌──────────────────┐         ┌──────────────────────────┐ │
│  │ PipelineDebug    │         │  PipelineInspector       │ │
│  │ Scope/Context    │         │  <TContext>              │ │
│  └──────────────────┘         └──────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## Summary

The Pipeline System provides:

- ✅ Declarative, composable middleware registration
- ✅ Async-first execution model
- ✅ Optional debug instrumentation
- ✅ Runtime inspection and dynamic modification
- ✅ Branching and conditional execution primitives
- ✅ High performance with minimal allocations

### Well-Suited For

- Web API request pipelines
- Multi-tenant service processing
- Event or command handlers
- Domain workflows requiring structured middleware execution
- Observability and debugging of async flows

---

## Next Steps

- **[Basic Usage](Pipeline-Basic-Usage.md)** - Learn basic patterns
- **[Pipeline Guide](Pipeline-Guide.md)** - Complete development guide
- **[API Reference](Pipeline-API-Reference.md)** - Full API documentation
- **[Best Practices](Pipeline-Best-Practices.md)** - Tips and recommendations

---

> **Built with ❤️ for .NET developers**