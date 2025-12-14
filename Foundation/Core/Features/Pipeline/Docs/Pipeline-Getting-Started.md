# Pipeline – Getting Started

This guide walks you through installing, configuring, and executing your first pipeline using the **Pipeline System**.

The goal is to get you productive quickly, while building a correct mental model of how the pipeline works.

---

## Installation

Add the Pipeline package to your project:

```bash
dotnet add package Core.Features.Pipeline
```

> The pipeline targets **.NET 10.0 (LTS)** and is forward-compatible with newer runtimes.

---

## Core Concepts (At a Glance)

Before writing code, it is important to understand the three core roles:

* **PipelineBuilder** – defines *what* runs and *in what order*
* **PipelineExecutor** – executes a compiled, immutable pipeline
* **Middleware** – small, composable execution units

```text
Configure  →  Build  →  Execute
```

---

## Step 1: Define a Context

The pipeline operates on a **context object** passed through all middleware.

```csharp
public sealed class RequestContext
{
    public string UserId { get; init; } = null!;
    public string TenantId { get; init; } = null!;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}
```

**Guidelines:**

* Context should be lightweight
* Prefer immutable (`init`) properties
* Avoid embedding services or infrastructure

---

## Step 2: Create Middleware

Middleware implements `IMiddleware<TContext>` and controls execution via `next()`.

```csharp
using Core.Features.Pipeline.Abstractions.Middleware;

public sealed class LoggingMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Request started for {context.UserId}");
        
        await next();
        
        Console.WriteLine("Request completed");
    }
}
```

**Important rules:**

* Middleware should be stateless
* Calling `next()` is optional
* Middleware fully controls continuation

---

## Step 3: Build the Pipeline

Use `PipelineBuilder<TContext>` to define execution order.

```csharp
using Core.Features.Pipeline.Runtime;

var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new ProcessingMiddleware());
```

At this stage:

* No execution occurs
* No allocations for runtime delegates
* The pipeline is purely declarative

---

## Step 4: Conditional Middleware

You can conditionally execute middleware using `UseWhen`.

```csharp
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);
```

This avoids `if` logic inside middleware and keeps behavior declarative.

---

## Step 5: Branch Pipelines

Branch pipelines allow isolated sub-flows.

```csharp
builder.UseBranch(
    ctx => ctx.TenantId == "tenant-enterprise",
    branch =>
    {
        branch.Use(new EnterpriseFeaturesMiddleware());
        branch.Use(new DedicatedSupportMiddleware());
    }
);
```

Branches:

* Have their own middleware order
* Share the same context
* Do not affect the parent pipeline

---

## Step 6: Execute the Pipeline

Once configured, create a `PipelineExecutor`.

```csharp
var executor = new PipelineExecutor<RequestContext>(builder);

await executor.ExecuteAsync(new RequestContext
{
    UserId = "user-123",
    TenantId = "tenant-enterprise",
    Roles = new[] { "Admin" }
});
```

**Execution guarantees:**

* Sequential execution
* Async-safe
* Immutable execution graph

---

## Optional: Enable Diagnostics

Diagnostics are **fully optional** and introduce **zero overhead** when disabled.

```csharp
using Core.Features.Pipeline.Diagnostics;

using var scope = PipelineDebugScope.Begin(out var debug);

await executor.ExecuteAsync(context);

foreach (var step in debug.Steps)
{
    Console.WriteLine(
        $"{step.Middleware.GetType().Name} | " +
        $"{step.Duration?.TotalMilliseconds:F2}ms | " +
        $"Next={step.NextCalled}"
    );
}
```

Diagnostics capture:

* Middleware execution order
* Execution duration
* Whether `next()` was called

---

## 🔍 What’s Next?

Now that you have a working pipeline, continue with:

* **[Basic Usage](Pipeline-Basic-Usage.md)** – common patterns
* **[Core Concepts](Pipeline-Concepts.md)** – execution model and internals
* **[Pipeline Guide](Pipeline-Guide.md)** – advanced scenarios
* **[Best Practices](Pipeline-Best-Practices.md)** – production guidance

---

> **Built with ❤️ for .NET developers**