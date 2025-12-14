# Pipeline API Reference – Core.Features.Pipeline.Abstractions.Middleware

This document describes the **Core.Features.Pipeline.Abstractions.Middleware** namespace, which defines the fundamental middleware contracts used by the Pipeline system.

---

## Namespace
```csharp
using Core.Features.Pipeline.Abstractions.Middleware;
```

## Overview

The middleware abstraction layer defines how individual pipeline components are structured, executed, and optionally described for inspection or diagnostics purposes.

This namespace is intentionally minimal and framework-agnostic, enabling:
* High-performance execution
* Strong typing via context generics
* Clear separation between runtime logic and diagnostics
* Compatibility with inspection and debugging layers

## Interfaces
**IMiddleware<TContext>**
```csharp
public interface IMiddleware<TContext>
```
**Purpose:**

Describes middleware component in a declarative, runtime-inspectable form, without exposing its implementation.

The descriptor allows the pipeline runtime, inspectors, and debugging tools to reason about pipeline structure and execution semantics independently of concrete middleware instances.

**Responsibilities:**
* Provide metadata about middleware behavior
* Enable pipeline inspection and visualization
* Describe execution semantics (terminal, conditional, branching)
* Decouple runtime analysis from middleware implementation
* Support diagnostics, debugging, and tooling scenarios

---

## Methods
### **InvokeAsync**
```csharp
ValueTask InvokeAsync(
    TContext context,
    PipelineDelegate<TContext> next
);
```

Description:
Executes the middleware logic and optionally invokes the next middleware in the pipeline.

**Parameters:**
* `context` – The strongly-typed pipeline context.
* `next` – Delegate representing the next middleware in the execution chain.

**Execution Rules:**
* The middleware must explicitly call **next(context)** to continue the pipeline.
* Omitting the call to `next` will stop further execution.
* The method must be async-safe and non-blocking.

**Example:**
```csharp
public sealed class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async ValueTask InvokeAsync(
        RequestContext context,
        PipelineDelegate<RequestContext> next)
    {
        Log.Start(context);

        await next(context);

        Log.End(context);
    }
}
```

###  **IMiddlewareDescriptor**
```csharp
public interface IMiddlewareDescriptor
```

**Purpose:**
Provides metadata describing a middleware for inspection, diagnostics, or tooling.

This interface is not required for runtime execution and should not contain logic.

**Typical Use Cases:**
* Pipeline inspection
* Debug visualization
* Profiling and tracing
* Developer tooling and diagnostics UI
___

## Properties
```csharp
Type MiddlewareType { get; }
```

**Description:**

The concrete middleware type represented by this descriptor.

>Used for identification, diagnostics, and tooling.
---

```csharp
string Name { get; }
```
**Description:**

Human-readable middleware name.
___

```csharp
MiddlewareKind Kind { get; }
```

**Description:**

Semantic classification of the middleware.

This value allows the runtime and tooling to understand how the middleware participates in execution, such as:
* Standard execution
* Conditional logic
* Branching behavior
* Diagnostic or infrastructure concerns
---

```csharp
bool IsTerminal { get; }
```
**Description:**

Indicates whether the middleware terminates pipeline execution.

**Semantics:**

* If `true`, the middleware is expected not to call `next()`
* Typically, represents final handlers or dispatchers
___

```csharp
bool IsConditional { get; }
```
**Description:**

Indicates whether the middleware executes conditionally based on runtime context.

Includes constructs such as:
* `UseWhen`
* Guarded execution
* Conditional branches
___
```csharp
IReadOnlyDictionary<string, object?> Metadata { get; }
```


**Description:**

Arbitrary, extensible metadata associated with the middleware.

**Rules:**
* Intended for diagnostics, tooling, and runtime extensions
* Must not be required for core execution semantics
* Values should be immutable or treated as read-only
___

**Example**
```csharp
public sealed class LoggingMiddleware :
    IMiddleware<RequestContext>,
    IMiddlewareDescriptor
{
    public Type MiddlewareType => typeof(LoggingMiddleware);
    public string Name => "Logging";
    public MiddlewareKind Kind => MiddlewareKind.Diagnostic;
    public bool IsTerminal => false;
    public bool IsConditional => false;

    public IReadOnlyDictionary<string, object?> Metadata =>
        new Dictionary<string, object?>
        {
            ["Level"] = "Information"
        };

    public async ValueTask InvokeAsync(MyContext context, Func<Task> next)
    {
        Log.Start(context);
        await next(context);
        Log.End(context);
    }
}

```
___
## MiddlewareKind Enum
```csharp
public enum MiddlewareKind
```
Defines the **semantic category** of a middleware component within a pipeline.

**Used for:**
* Runtime inspection
* Diagnostics
* Debugging
* Pipeline visualization

**Values**
* **Standard** – participates in normal execution and calls next()
* **Conditional** – executes conditionally based on runtime context
* **Branch** – executes an independent sub-pipeline
* **Terminal** – ends pipeline execution
* **Diagnostic** – logging, tracing, metrics, debugging


## Design Notes
* No inheritance hierarchy – middleware composition is favored over inheritance.
* No I/O in descriptors – metadata must remain side effect free.
* ValueTask-based API – optimized for high-throughput scenarios.
* Descriptor is optional – runtime does not depend on metadata.

## Relationship to Other Components
* `IPipelineBuilder<TContext>` – registers middleware instances.
* `PipelineExecutor<TContext>` – executes middleware in order.
* `PipelineInspector<TContext>` – reads metadata via IMiddlewareDescriptor.
* `PipelineDebuggerMiddleware` – consumes descriptors for diagnostics only.
___

This namespace defines the core execution contract of the Pipeline system.
All higher-level features build upon these abstractions.

> **Built with ❤️ for .NET developers**