# Pipeline API Reference – Core.Features.Pipeline.Abstractions

This document describes the **Core.Features.Pipeline.Abstractions** namespace, including the `IPipelineBuilder<TContext>` interface for constructing flexible, conditional, and branch able middleware pipelines.

---

## Namespace
```csharp
using Core.Features.Pipeline.Abstractions;
using Core.Features.Pipeline.Abstractions.Middleware;
```

## Interface Overview
**IPipelineBuilder<TContext>**
```csharp
public interface IPipelineBuilder<TContext>
```
**Purpose:**
Provides declarative API to configure middleware pipelines for a specific context type.

**Key Features:**
* Add middleware to start, end, before, or after other middleware
* Conditional execution based on context
* Branch pipelines for isolated conditional flows
* Supports pipelines for requests, events, or commands
* 
---

### Methods
1. **Use**
```csharp
void Use(IMiddleware<TContext> middleware);
```
**Description:**

Adds middleware to the end of the pipeline.

**Example:**
```csharp
builder.Use(new LoggingMiddleware());
```

2. **UseFirst**
```csharp
   void UseFirst(IMiddleware<TContext> middleware);
```
**Description:**

Adds middleware to the beginning of the pipeline.

**Example:**
```csharp
builder.UseFirst(new ExceptionHandlingMiddleware());
```

3. **UseAfter**
```csharp
void UseAfter(Predicate<IMiddleware<TContext>> predicate, IMiddleware<TContext> middleware);
```
**Description:**

Inserts middleware immediately after the first middleware matching a predicate.

**Parameters:**
* `predicate` – Function to locate the reference middleware.
* `middleware` – Middleware to insert.

**Example:**
```csharp
builder.UseAfter(
    m => m is LoggingMiddleware,
    new AuditMiddleware()
);
```

4. **UseBefore**
```csharp
void UseBefore(Predicate<IMiddleware<TContext>> predicate, IMiddleware<TContext> middleware);
```
**Description:**

Inserts middleware immediately before the first middleware matching a predicate.

**Example:**
```csharp
builder.UseBefore(
    m => m is ValidationMiddleware,
    new PreValidationMiddleware()
);
```

5. **UseWhen**
```csharp
void UseWhen(Func<TContext, bool> condition, IMiddleware<TContext> middleware);
```
**Description:**

Adds middleware that executes only when condition on the context evaluates to true.

**Example:**
```csharp
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);
```

6. **UseBranch**
```csharp
void UseBranch(
    Func<TContext, bool> condition,
    Action<IPipelineBuilder<TContext>> configurePipeline
);
```


**Description**:

Creates conditional branch in the pipeline. The branch executes separate pipeline configuration when the condition is true.

**Example:**
```csharp
builder.UseBranch(
    ctx => ctx.TenantId == "premium",
    branch =>
    {
        branch.Use(new PremiumFeaturesMiddleware());
        branch.Use(new AnalyticsMiddleware());
    }
);
```

## Notes

* **Execution Order**: Middlewares are executed in the order they are registered.
* **Conditional Middleware**: Both `UseWhen` and `UseBranch` allow context-aware selective execution.
* **Branch Pipelines**: Useful for multi-tenant logic, feature flags, or role-based flows.
* **Type Safety**: Pipelines are strongly typed by context (`TContext`).

## Typical Usage Pattern
```csharp
var builder = new PipelineBuilder<RequestContext>();

// Always executed
builder.Use(new LoggingMiddleware());
builder.UseFirst(new ExceptionHandlingMiddleware());

// Conditional execution
builder.UseWhen(ctx => ctx.IsAuthenticated, new AuthenticationMiddleware());

// Branch pipelines
builder.UseBranch(
    ctx => ctx.TenantId.StartsWith("premium"),
    branch =>
    {
        branch.Use(new PremiumFeaturesMiddleware());
        branch.Use(new AnalyticsMiddleware());
    }
);
```

This provides composable, readable, and maintainable way to define complex middleware pipelines.

> **Built with ❤️ for .NET developers**