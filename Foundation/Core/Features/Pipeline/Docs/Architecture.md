# Pipeline System - Architecture

Comprehensive overview of the internal architecture of the Pipeline System, designed for deterministic, async-safe, and extensible middleware execution.

---

## Table of Contents

- [Architectural Principles](#architectural-principles)
- [Component Architecture](#component-architecture)
- [Execution Model](#execution-model)
- [Dependency Injection](#dependency-injection)
- [Design Decisions](#design-decisions)
- [Use Cases](#use-cases)

---

## Architectural Principles

The architecture follows five foundational principles:

### 1. Single-Purpose Middleware

Each middleware performs exactly one well-defined task.

**Benefits:**
- ✅ Easy to test and debug
- ✅ Reusable across multiple pipelines
- ✅ Predictable execution order
- ✅ Clear separation of responsibilities

**Example:**
```csharp
// ✅ Single purpose - only logs
public class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        _logger.LogInformation("Request started");
        await next();
        _logger.LogInformation("Request completed");
    }
}

// ✅ Single purpose - only validates
public class ValidationMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        if (!IsValid(context))
            throw new ValidationException();
        await next();
    }
}
```

---

### 2. Statelessness and Thread Safety

Middleware instances are stateless and thread-safe.

**Requirements:**
- ✅ No mutable fields affecting execution
- ✅ Safe across concurrent requests
- ✅ Async-safe without exposing internal flow control
- ✅ All dependencies injected via constructor

**Example:**
```csharp
// ✅ Good - stateless
public class AuthenticationMiddleware : IMiddleware<RequestContext>
{
    private readonly IAuthService _authService;  // ✅ Immutable dependency
    
    public AuthenticationMiddleware(IAuthService authService)
    {
        _authService = authService;
    }
}

// ❌ Bad - stateful
public class BadMiddleware : IMiddleware<RequestContext>
{
    private int _requestCount = 0;  // ❌ Mutable state - race condition!
}
```

---

### 3. Deterministic Pipeline Execution

Pipeline execution is controlled and deterministic.

**Guarantees:**
- ✅ Execution order defined explicitly
- ✅ Branching is pre-determined and controlled
- ✅ Conditional middleware evaluated declaratively via `UseWhen`
- ✅ `next()` guarantees continuation unless explicitly stopped

**Example:**
```csharp
var builder = new PipelineBuilder<RequestContext>();

// Execution order: 1 → 2 → 3
builder.Use(new LoggingMiddleware());        // 1
builder.Use(new ValidationMiddleware());     // 2
builder.Use(new ProcessingMiddleware());     // 3

// Deterministic - always executes in this order
var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(context);
```

---

### 4. Branch Isolation

Branch pipelines isolate conditional logic.

**Characteristics:**
- ✅ Tenant-specific, role-specific, or feature-specific pipelines
- ✅ Nested branches allowed (max 2-3 levels recommended)
- ✅ Each branch independently testable and auditable
- ✅ No side effects propagate outside branch

**Example:**
```csharp
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        // Isolated enterprise-only pipeline
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);
// Branch does not affect main pipeline
```

---

### 5. Minimal Runtime Overhead

The pipeline is optimized for production performance.

**Optimizations:**
- ✅ Pipeline structure immutable during execution
- ✅ Avoid heavy I/O, synchronous calls, or allocations inside middleware
- ✅ Optional debug instrumentation with zero-cost when disabled
- ✅ Inspector tools for runtime analysis without modifying pipeline

**Performance Goal:** ~100ns overhead per middleware when debugging is disabled.

---

## Component Architecture

### PipelineBuilder\<TContext\>

Responsible for declaring pipeline structure.

**Responsibilities:**
- Registers middleware in order
- Supports conditional middleware (`UseWhen`)
- Supports branch pipelines (`UseBranch`)
- Immutable once executor is created

**API:**
```csharp
public class PipelineBuilder<TContext>
{
    public List<IMiddleware<TContext>> Middlewares { get; }
    
    public PipelineBuilder<TContext> Use(IMiddleware<TContext> middleware);
    public PipelineBuilder<TContext> UseWhen(
        Func<TContext, bool> predicate,
        IMiddleware<TContext> middleware);
    public PipelineBuilder<TContext> UseBranch(
        Func<TContext, bool> predicate,
        Action<PipelineBuilder<TContext>> configureBranch);
}
```

**Example:**
```csharp
var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.UseWhen(ctx => ctx.IsAdmin, new AdminAuditMiddleware());
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    branch => branch.Use(new EnterpriseFeaturesMiddleware())
);
```

---

### PipelineExecutor\<TContext\>

Responsible for executing the pipeline.

**Responsibilities:**
- Takes `TContext` as input
- Calls each middleware sequentially
- Handles `next()` propagation
- Manages branch traversal deterministically

**API:**
```csharp
public class PipelineExecutor<TContext>
{
    public PipelineExecutor(PipelineBuilder<TContext> builder);
    
    public Task ExecuteAsync(
        TContext context,
        CancellationToken cancellationToken = default);
}
```

**Example:**
```csharp
var executor = new PipelineExecutor<RequestContext>(builder);

var context = new RequestContext
{
    UserId = "user-123",
    TenantId = "tenant-456"
};

await executor.ExecuteAsync(context);
```

---

### IMiddleware\<TContext\>

Interface defining a single middleware unit.

**Definition:**
```csharp
public interface IMiddleware<TContext>
{
    Task InvokeAsync(
        TContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default);
}
```

**Key Constraints:**
- ✅ Stateless and single-purpose
- ✅ Lightweight operations only
- ✅ Dependencies injected via constructor
- ✅ Always call `next()` unless short-circuiting is intended

**Example:**
```csharp
public class ExampleMiddleware : IMiddleware<RequestContext>
{
    private readonly ILogger _logger;
    
    public ExampleMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Before");
        await next();  // Continue pipeline
        _logger.LogInformation("After");
    }
}
```

---

### Branch Pipelines

Branches allow isolated conditional execution.

**Architecture:**
```csharp
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseMiddleware());
        enterpriseBranch.Use(new SupportMiddleware());
    }
);
```

**Requirements:**
- ✅ Predicate must be simple, deterministic, and side-effect-free
- ✅ Nested branches allowed with caution
- ✅ Each branch independently testable

**Execution Flow:**
```
Request
    │
    ▼
Logging Middleware
    │
    ▼
Branch Predicate: ctx.Plan == "Enterprise"?
    │
    ├─ Yes → Enterprise Branch
    │   ├─ Enterprise Middleware
    │   └─ Support Middleware
    │
    └─ No → Skip Branch
    │
    ▼
Processing Middleware
    │
    ▼
Response
```

---

### Conditional Middleware

Declarative conditional execution via `UseWhen`.

**Architecture:**
```csharp
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);
```

**Guidelines:**
- ✅ Avoid complex or non-deterministic predicates
- ✅ Keep branching logic out of middleware itself
- ✅ Predicates evaluated for each request

**Execution Flow:**
```
Request
    │
    ▼
Predicate: ctx.Roles.Contains("Admin")?
    │
    ├─ Yes → Execute AdminAuditMiddleware
    │
    └─ No → Skip Middleware
    │
    ▼
Next Middleware
```

---

### PipelineInspector\<TContext\>

Runtime tool for analysis and diagnostics.

**Capabilities:**
- Enumerates middleware and branch structure
- Provides metadata descriptors (`IMiddlewareDescriptor`)
- Allows safe removal or inspection prior to execution
- Thread-safe and does not alter active pipeline

**API:**
```csharp
public class PipelineInspector<TContext>
{
    public IEnumerable<IMiddleware<TContext>> GetMiddlewares();
    public IEnumerable<IMiddlewareDescriptor> GetDescriptors();
    public void Remove(Func<IMiddleware<TContext>, bool> predicate);
    public void Clear();
}
```

**Example:**
```csharp
var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// List middleware
foreach (var mw in inspector.GetMiddlewares())
{
    Console.WriteLine(mw.GetType().Name);
}

// Remove middleware
inspector.Remove(mw => mw is DebugMiddleware);

// Get metadata
foreach (var desc in inspector.GetDescriptors())
{
    Console.WriteLine($"{desc.Name} [{desc.Kind}]");
}
```

---

## Execution Model

### Execution Flow Diagram
```
Application Code
      │
      │ creates
      ▼
[RequestContext]
      │
      │ passed into executor
      ▼
PipelineExecutor.ExecuteAsync
      │
      │ iterates middleware
      ▼
┌─────────────────────────────────────┐
│     Middleware Chain                │
│                                     │
│  Middleware 1                       │
│      │                              │
│      ├─ Conditional?                │
│      │   ├─ Yes → Execute           │
│      │   └─ No → Skip               │
│      │                              │
│      ├─ Branch?                     │
│      │   ├─ Yes → Execute Branch    │
│      │   └─ No → Skip               │
│      │                              │
│      ▼                              │
│  Middleware 2                       │
│      │                              │
│      ▼                              │
│  Middleware N                       │
└─────────────────────────────────────┘
      │
      │ execution complete
      ▼
Response
```

### Key Characteristics

**Execution is fully deterministic:**
- Middleware executes in registration order
- Conditional paths resolved before invocation
- Branch evaluation happens at runtime but follows deterministic rules

**Debug instrumentation:**
- Only active if explicitly enabled via `PipelineDebugScope`
- Zero overhead when disabled

---

## Dependency Injection

### Registration
```csharp
// Register pipeline infrastructure
services.AddSingleton<PipelineBuilder<RequestContext>>(sp =>
{
    var builder = new PipelineBuilder<RequestContext>();
    builder.Use(new LoggingMiddleware());
    builder.Use(new ValidationMiddleware());
    return builder;
});

services.AddSingleton<PipelineExecutor<RequestContext>>(sp =>
{
    var builder = sp.GetRequiredService<PipelineBuilder<RequestContext>>();
    return new PipelineExecutor<RequestContext>(builder);
});

// Optional: Register inspector for dev/test
services.AddSingleton<PipelineInspector<RequestContext>>(sp =>
{
    var builder = sp.GetRequiredService<PipelineBuilder<RequestContext>>();
    return new PipelineInspector<RequestContext>(builder.Middlewares);
});
```

**Lifetimes:**
- `PipelineBuilder<TContext>` - Singleton
- `PipelineExecutor<TContext>` - Singleton
- Middleware dependencies - Injected via constructor (any lifetime)

---

## Design Decisions

### Why Not Modify Pipeline at Runtime?

**Reasons:**
- ❌ Dynamic modification risks thread-safety issues
- ❌ Can cause inconsistent diagnostics
- ❌ Non-deterministic execution
- ❌ Hard to test and debug

**Recommended Approach:**
- ✅ Inspect and modify pipeline **before** execution using `PipelineInspector`
- ✅ Build pipeline once, execute many times
- ✅ Use conditional middleware and branches for runtime flexibility

---

### Why Not Heavy Operations in Middleware?

**Reasons:**
- ❌ Synchronous DB calls block async execution
- ❌ Large I/O increases latency
- ❌ CPU-intensive tasks reduce throughput

**Recommended Approach:**
- ✅ Middleware should be lightweight
- ✅ Delegate heavy tasks to services
- ✅ Ensures minimal latency and predictable throughput

**Example:**
```csharp
// ❌ Bad - heavy operation in middleware
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        var data = await _db.Users
            .Include(u => u.Orders)
            .ToListAsync();  // ❌ Heavy query
        
        await next();
    }
}

// ✅ Good - delegate to service
public class GoodMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ✅ Lightweight check
        if (!await _validator.IsValidAsync(context))
            throw new ValidationException();
        
        await next();
    }
}
```

---

### Why AsyncLocal Only for Debugging?

**Reasons:**
- ✅ `AsyncLocal` has overhead for storage and cleanup
- ✅ Not needed for normal pipeline execution
- ✅ Zero-cost abstraction when disabled

**Implementation:**
- Debug instrumentation uses `AsyncLocal` only when `PipelineDebugScope` is active
- Normal execution has no `AsyncLocal` overhead

---

## Use Cases

The Pipeline System is well-suited for:

### 1. Multi-Tenant Backend Services
```csharp
builder.UseBranch(
    ctx => ctx.TenantId.StartsWith("enterprise-"),
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
    }
);
```

### 2. Feature-Flagged Execution Paths
```csharp
builder.UseWhen(
    ctx => ctx.Features.Contains("BetaAccess"),
    new BetaFeaturesMiddleware()
);
```

### 3. Request/Command Validation Pipelines
```csharp
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new AuthenticationMiddleware());
builder.Use(new AuthorizationMiddleware());
builder.Use(new ProcessingMiddleware());
```

### 4. Event-Driven Pipelines
```csharp
// Message bus handler pipeline
builder.Use(new MessageDeserializationMiddleware());
builder.Use(new MessageValidationMiddleware());
builder.Use(new MessageProcessingMiddleware());
builder.Use(new MessageAcknowledgmentMiddleware());
```

### 5. Background Job Orchestration
```csharp
builder.Use(new JobLoggingMiddleware());
builder.Use(new JobLockingMiddleware());
builder.Use(new JobExecutionMiddleware());
builder.Use(new JobCleanupMiddleware());
```

### 6. Plugin or Sandboxed Execution Flows
```csharp
builder.Use(new PluginAuthenticationMiddleware());
builder.Use(new PluginSandboxMiddleware());
builder.Use(new PluginExecutionMiddleware());
builder.Use(new PluginCleanupMiddleware());
```

---

## Summary

The Pipeline System architecture provides:

- ✅ **Stateless, single-purpose, testable middleware units**
- ✅ **Deterministic execution with conditional and branch support**
- ✅ **Async-safe propagation of context** (`TContext`)
- ✅ **Minimal runtime overhead** with optional debug instrumentation
- ✅ **Clear separation** between pipeline definition, execution, and diagnostics

**This design ensures production-grade reliability, maintainability, and performance for complex .NET applications.**

---

## See Also

- [Getting Started](Pipeline-Getting-Started.md) - Setup and basic usage
- [Core Concepts](Pipeline-Concepts.md) - Understanding the fundamentals
- [Best Practices](Pipeline-Best-Practices.md) - Recommended patterns
- [API Reference](Pipeline-API-Reference.md) - Complete API documentation

---

> **Built with ❤️ for .NET developers**