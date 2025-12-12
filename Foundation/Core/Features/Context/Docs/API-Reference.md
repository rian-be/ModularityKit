# API Reference

Complete API documentation for the Context System.

---

## Table of Contents

- [Interfaces](#interfaces)
  - [IContext](#icontext)
  - [IReadOnlyContext](#ireadonlycontext)
  - [IContextAccessor\<TContext\>](#icontextaccessortcontext)
  - [IContextManager\<TContext\>](#icontextmanagertcontext)
- [Extension Methods](#extension-methods)
  - [AddContext\<TContext\>()](#addcontexttcontext)
  - [AddReadOnlyContextAccessor\<TContext\>()](#addreadonlycontextaccessortcontext)
- [Classes](#classes)
  - [ContextStore\<TContext\>](#contextstoretcontext)
  - [ContextAccessor\<TContext\>](#contextaccessortcontext)
  - [ContextManager\<TContext\>](#contextmanagertcontext)
  - [ReadOnlyContextSnapshot](#readonlycontextsnapshot)
  - [ReadOnlyContextAccessor\<TContext\>](#readonlycontextaccessortcontext)
- [Exceptions](#exceptions)
- [Complete Examples](#complete-examples)

---

## Interfaces

### IContext

Base interface that all contexts must implement.

**Namespace:** `Core.Features.Context.Abstractions`
```csharp
public interface IContext
{
    /// <summary>
    /// Unique identifier for this context instance.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Timestamp when the context was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique context identifier (recommended: GUID or ULID) |
| `CreatedAt` | `DateTimeOffset` | UTC timestamp when the context was created |

**Implementation Example:**
```csharp
public class MyContext(string id, string userId, string tenantId) : IContext
{
    public string Id { get; } = id;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    // Additional properties
    public string UserId { get; } = userId;
    public string TenantId { get; } = tenantId;

    // Factory method
    public static MyContext Create(string userId, string tenantId)
    {
        return new MyContext(
            id: Guid.NewGuid().ToString("N"),
            userId: userId,
            tenantId: tenantId
        );
    }
}
```

**Best Practices:**

- ✅ Use GUID or ULID for `Id` to ensure uniqueness
- ✅ Always set `CreatedAt` to `DateTimeOffset.UtcNow`
- ✅ Treat context as immutable after creation
- ✅ Use factory methods for convenient creation

---

### IReadOnlyContext

Read-only view of context for untrusted code (plugins, third-party modules).

**Namespace:** `Core.Features.Context.Abstractions`
```csharp
public interface IReadOnlyContext : IContext
{
    // Inherits Id and CreatedAt from IContext
    // No additional members (security boundary)
}
```

**Purpose:**

Provides a security boundary between trusted and untrusted code. Untrusted code receives only safe, read-only properties without access to sensitive data or methods.

**Usage in Untrusted Code:**
```csharp
public class ThirdPartyPlugin(IContextAccessor<IReadOnlyContext> context)
{
    private readonly IContextAccessor<IReadOnlyContext> _context = context;
    
    public void Execute()
    {
        var ctx = _context.RequireCurrent();
        
        // ✅ Available - safe properties
        Console.WriteLine($"Context ID: {ctx.Id}");
        Console.WriteLine($"Created: {ctx.CreatedAt}");
        
        // ❌ Not available - sensitive properties
        // Console.WriteLine(ctx.UserId);    // Compilation error
        // Console.WriteLine(ctx.TenantId);  // Compilation error
    }
}
```

**Security Guarantees:**

- ✅ Cannot be cast back to full context type
- ✅ Cannot access sensitive properties via reflection
- ✅ Cannot call sensitive methods
- ✅ Receives defensive copy (snapshot), not reference

---

### IContextAccessor\<TContext\>

Provides type-safe access to the current active context.

**Namespace:** `Core.Features.Context.Abstractions`
```csharp
public interface IContextAccessor<out TContext> where TContext : class, IContext
{
    /// <summary>
    /// Gets the current context, or null if no context is active.
    /// </summary>
    TContext? Current { get; }
    
    /// <summary>
    /// Gets the current context, throwing if no context is active.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no context is currently active.
    /// </exception>
    TContext RequireCurrent();
}
```

**Type Parameter:**

| Parameter | Constraint | Description |
|-----------|------------|-------------|
| `TContext` | `: class, IContext` | The specific context type to access |

**Why Generic?**

The generic design enables strong type isolation between trust boundaries:

- **Trusted code:** `IContextAccessor<MyContext>` - full access to all properties and methods
- **Untrusted code:** `IContextAccessor<IReadOnlyContext>` - sandboxed read-only projection
- **Custom contexts:** `IContextAccessor<PipelineContext>`, `IContextAccessor<SagaContext>`, etc.

**Members:**

| Member | Returns | Throws | Description |
|--------|---------|--------|-------------|
| `Current` | `TContext?` | No | Returns active context or `null` |
| `RequireCurrent()` | `TContext` | `InvalidOperationException` | Returns active context, never `null` |

**Usage Examples:**

**Trusted Service (Full Access):**
```csharp
public class OrderService(IContextAccessor<MyContext> context)
{
    private readonly IContextAccessor<MyContext> _context = context;
    
    public void ProcessOrder()
    {
        var ctx = _context.RequireCurrent();
        
        // ✅ Full access to all properties
        Console.WriteLine($"User: {ctx.UserId}");
        Console.WriteLine($"Tenant: {ctx.TenantId}");
        
        // ✅ Can call sensitive methods
        ctx.DoSensitiveOperation();
    }
}
```

**Untrusted Plugin (Read-Only Access):**
```csharp
public class ThirdPartyPlugin(IContextAccessor<IReadOnlyContext> context)
{
    private readonly IContextAccessor<IReadOnlyContext> _context = context;
    
    public void Execute()
    {
        var ctx = _context.RequireCurrent();
        
        // ✅ Limited access - only safe properties
        Console.WriteLine($"Context: {ctx.Id}");
        Console.WriteLine($"Created: {ctx.CreatedAt}");
        
        // ❌ Cannot access sensitive data (compilation error)
        // Console.WriteLine(ctx.UserId);
    }
}
```

**Safe Access with Null Check:**
```csharp
public class OptionalContextService(IContextAccessor<MyContext> context)
{
    public void ProcessIfAvailable()
    {
        var ctx = _context.Current;
        
        if (ctx != null)
        {
            Console.WriteLine($"Processing for: {ctx.UserId}");
        }
        else
        {
            Console.WriteLine("No active context - using defaults");
        }
    }
}
```

**When to use which:**

| Use Case | Recommended |
|----------|-------------|
| Context is optional | `Current` with null check |
| Context is required | `RequireCurrent()` |
| Top-level method | `RequireCurrent()` to fail fast |
| Helper method | `Current` to be more defensive |
| Trusted code path | `IContextAccessor<MyContext>` |
| Untrusted code path | `IContextAccessor<IReadOnlyContext>` |

---

### IContextManager\<TContext\>

Manages context lifecycle and execution scopes.

**Namespace:** `Core.Features.Context.Abstractions`
```csharp
public interface IContextManager<TContext> where TContext : class, IContext
{
    /// <summary>
    /// Gets the current active context.
    /// </summary>
    TContext? Current { get; }
    
    /// <summary>
    /// Executes an action within a context scope.
    /// </summary>
    /// <param name="context">Context to activate for the duration.</param>
    /// <param name="action">Action to execute with context active.</param>
    /// <returns>Task representing the async operation.</returns>
    Task ExecuteInContext(TContext context, Func<Task> action);
    
    /// <summary>
    /// Executes a function within a context scope and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Return type of the function.</typeparam>
    /// <param name="context">Context to activate for the duration.</param>
    /// <param name="func">Function to execute with context active.</param>
    /// <returns>Task containing the result of the function.</returns>
    Task<TResult> ExecuteInContext<TResult>(TContext context, Func<Task<TResult>> func);
}
```

**Type Parameter:**

| Parameter | Constraint | Description |
|-----------|------------|-------------|
| `TContext` | `: class, IContext` | The specific context type to manage |

**Members:**

| Member | Description | Behavior |
|--------|-------------|----------|
| `Current` | Gets the currently active context | Returns `null` if no context is active |
| `ExecuteInContext(context, action)` | Executes action with context | Context is active during execution, automatically cleaned up after |
| `ExecuteInContext<T>(context, func)` | Executes function and returns result | Context is active during execution, automatically cleaned up after |

**Execute Action Example:**
```csharp
public class RequestHandler(
    IContextManager<RequestContext> manager,
    IOrderService orderService)
{
    public async Task HandleRequest(HttpContext http)
    {
        var context = RequestContext.FromHttpContext(http);
        
        // Execute with context active
        await manager.ExecuteInContext(context, async () =>
        {
            await orderService.CreateOrder();
            await orderService.SendConfirmation();
            // Context is active for all operations
        });
        // Context automatically cleaned up here
    }
}
```

**Execute with Result Example:**
```csharp
public class RequestHandler(
    IContextManager<RequestContext> manager,
    IOrderService orderService)
{
    public async Task<Order> HandleRequest(HttpContext http)
    {
        var context = RequestContext.FromHttpContext(http);
        
        // Execute and get result
        var order = await manager.ExecuteInContext(context, async () =>
        {
            return await orderService.CreateOrder();
        });
        
        return order;
    }
}
```

**Context Cleanup Guarantees:**

- ✅ Context is cleaned up even if exception is thrown
- ✅ Nested contexts are properly restored
- ✅ Thread-safe and async-safe

---

## Extension Methods

### AddContext\<TContext\>()

Registers context infrastructure in the dependency injection container.

**Namespace:** `Core.Features.Context.Extensions`
```csharp
public static IServiceCollection AddContext<TContext>(
    this IServiceCollection services)
    where TContext : class, IContext
```

**Type Parameter:**

| Parameter  | Constraint          | Description                  |
|------------|---------------------|------------------------------|
| `TContext` | `: class, IContext` | The context type to register |

**Parameters:**

| Parameter  | Type                 | Description                          |
|------------|----------------------|--------------------------------------|
| `services` | `IServiceCollection` | DI container to register services in |

**Returns:** `IServiceCollection` for method chaining

**Registers:**

| Service                      | Lifetime  | Description              |
|------------------------------|-----------|--------------------------|
| `ContextStore<TContext>`     | Singleton | Internal context storage |
| `IContextAccessor<TContext>` | Singleton | Access current context   |
| `IContextManager<TContext>`  | Singleton | Manage context lifecycle |

**Usage:**
```csharp
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register context infrastructure
services.AddContext<MyAppContext>();

var serviceProvider = services.BuildServiceProvider();
```

**Multiple Context Types:**
```csharp
// Register multiple context types
services.AddContext<RequestContext>();
services.AddContext<JobContext>();
services.AddContext<TenantContext>();
```

---

### AddReadOnlyContextAccessor\<TContext\>()

Registers read-only context accessor for untrusted code (plugins, third-party modules).

**Namespace:** `Core.Features.Context.Extensions`
```csharp
public static IServiceCollection AddReadOnlyContextAccessor<TContext>(
    this IServiceCollection services)
    where TContext : class, IContext
```

**Type Parameter:**

| Parameter  | Constraint          | Description                                       |
|------------|---------------------|---------------------------------------------------|
| `TContext` | `: class, IContext` | The context type to create read-only accessor for |

**Parameters:**

| Parameter  | Type                 | Description                          |
|------------|----------------------|--------------------------------------|
| `services` | `IServiceCollection` | DI container to register services in |

**Returns:** `IServiceCollection` for method chaining

**Registers:**

| Service                              | Lifetime  | Description                |
|--------------------------------------|-----------|----------------------------|
| `IContextAccessor<IReadOnlyContext>` | Singleton | Read-only context accessor |

**Usage:**
```csharp
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register full context
services.AddContext<MyAppContext>();

// Register read-only accessor for untrusted code
services.AddReadOnlyContextAccessor<MyAppContext>();

var serviceProvider = services.BuildServiceProvider();
```

**DI Setup for Trusted vs Untrusted:**
```csharp
// Trusted service - full access
services.AddTransient<OrderService>();
// Constructor: OrderService(IContextAccessor<MyAppContext> context)

// Untrusted plugin - read-only access
services.AddTransient<ThirdPartyPlugin>();
// Constructor: ThirdPartyPlugin(IContextAccessor<IReadOnlyContext> context)
```

---

## Classes

### ContextStore\<TContext\>

Internal storage for context instances using `AsyncLocal<T>`.

**Namespace:** `Core.Features.Context.Runtime`
```csharp
public sealed class ContextStore<TContext> where TContext : class, IContext
{
    /// <summary>
    /// Gets the currently active context, or null if no context is set.
    /// </summary>
    public TContext? Current { get; }
    
    /// <summary>
    /// Sets the current context and returns a disposable scope.
    /// </summary>
    /// <param name="context">Context to set as current.</param>
    /// <returns>Disposable scope that restores previous context on disposal.</returns>
    public IDisposable SetCurrent(TContext context);
    
    /// <summary>
    /// Clears the current context.
    /// </summary>
    public void Clear();
}
```

**Implementation Details:**

- Uses `AsyncLocal<T>` for automatic async propagation
- Provides `IDisposable` scope for automatic cleanup
- Thread-safe and async-safe

**Note:** This is an internal implementation detail. Use `IContextManager<TContext>` instead of directly manipulating the store.

---

### ContextAccessor\<TContext\>

Provides access to the current context via `ContextStore<TContext>`.

**Namespace:** `Core.Features.Context.Runtime`
```csharp
public sealed class ContextAccessor<TContext> : IContextAccessor<TContext>
    where TContext : class, IContext
{
    public ContextAccessor(ContextStore<TContext> store);
    
    public TContext? Current { get; }
    public TContext RequireCurrent();
}
```

**Note:** Registered automatically by `AddContext<TContext>()`.

---

### ContextManager\<TContext\>

Manages context lifecycle and execution scopes.

**Namespace:** `Core.Features.Context.Runtime`
```csharp
public sealed class ContextManager<TContext> : IContextManager<TContext>
    where TContext : class, IContext
{
    public ContextManager(ContextStore<TContext> store);
    
    public TContext? Current { get; }
    
    public async Task ExecuteInContext(TContext context, Func<Task> action);
    public async Task<TResult> ExecuteInContext<TResult>(TContext context, Func<Task<TResult>> func);
}
```

**Note:** Registered automatically by `AddContext<TContext>()`.

---

### ReadOnlyContextSnapshot

Immutable snapshot of context for untrusted code.

**Namespace:** `Core.Features.Context.ReadOnly`
```csharp
public sealed record ReadOnlyContextSnapshot : IReadOnlyContext
{
    public string Id { get; }
    public DateTimeOffset CreatedAt { get; }
    
    public ReadOnlyContextSnapshot(string id, DateTimeOffset createdAt);
    
    /// <summary>
    /// Creates a defensive copy snapshot from any IContext.
    /// </summary>
    public static ReadOnlyContextSnapshot FromContext(IContext context);
}
```

**Purpose:** Security boundary - provides defensive copy with only safe properties.

**Example:**
```csharp
// Full context
var fullContext = new MyContext(
    id: "ctx-123",
    userId: "user-456",
    tenantId: "tenant-789"
);

// Create snapshot (defensive copy)
var snapshot = ReadOnlyContextSnapshot.FromContext(fullContext);

// Snapshot only has safe properties
Console.WriteLine(snapshot.Id);         // ✅ "ctx-123"
Console.WriteLine(snapshot.CreatedAt);  // ✅ timestamp

// Cannot access sensitive properties
// snapshot.UserId;     // ❌ Compilation error
// snapshot.TenantId;   // ❌ Compilation error
```

---

### ReadOnlyContextAccessor\<TContext\>

Provides read-only access to contexts for untrusted code.

**Namespace:** `Core.Features.Context.ReadOnly`
```csharp
public sealed class ReadOnlyContextAccessor<TContext> : IContextAccessor<IReadOnlyContext>
    where TContext : class, IContext
{
    public ReadOnlyContextAccessor(IContextAccessor<TContext> innerAccessor);
    
    public IReadOnlyContext? Current { get; }
    public IReadOnlyContext RequireCurrent();
}
```

**Note:** Registered automatically by `AddReadOnlyContextAccessor<TContext>()`.

**Behavior:**

- Returns `ReadOnlyContextSnapshot` (defensive copy)
- No reference to original context
- Cannot be cast back to full context

---

## Exceptions

### InvalidOperationException

Thrown by `RequireCurrent()` when no context is active.
```csharp
public class OrderService(IContextAccessor<MyContext> context)
{
    public void ProcessOrder()
    {
        try
        {
            var ctx = context.RequireCurrent();
            // ... use context
        }
        catch (InvalidOperationException ex)
        {
            // No active context
            Console.WriteLine(ex.Message); // "No active context found"
        }
    }
}
```

**Prevention:**

Always execute within `ExecuteInContext`:
```csharp
await manager.ExecuteInContext(context, async () =>
{
    var ctx = accessor.RequireCurrent();  // ✅ Never throws
});
```

---

## Complete Examples

### Example 1: Basic Context Usage
```csharp
using Core.Features.Context.Abstractions;
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;

// 1. Define context
public class MyContext(string id, string userId) : IContext
{
    public string Id { get; } = id;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public string UserId { get; } = userId;
}

// 2. Register services
var services = new ServiceCollection();
services.AddContext<MyContext>();
services.AddTransient<OrderService>();

var sp = services.BuildServiceProvider();

// 3. Define service
public class OrderService(IContextAccessor<MyContext> context)
{
    public void ProcessOrder()
    {
        var ctx = context.RequireCurrent();
        Console.WriteLine($"Processing for: {ctx.UserId}");
    }
}

// 4. Use
var manager = sp.GetRequiredService<IContextManager<MyContext>>();
var service = sp.GetRequiredService<OrderService>();

var context = new MyContext("ctx-1", "user-123");

await manager.ExecuteInContext(context, async () =>
{
    service.ProcessOrder();
    await Task.CompletedTask;
});
```

### Example 2: Trust Boundary Demonstration
```csharp
// Setup DI
services.AddContext<MyContext>();
services.AddReadOnlyContextAccessor<MyContext>();
services.AddTransient<TrustedService>();
services.AddTransient<UntrustedService>();

// Trusted service - full access
public class TrustedService(IContextAccessor<MyContext> context)
{
    public void DoWork()
    {
        var ctx = context.RequireCurrent();
        
        // ✅ Full access
        Console.WriteLine($"User: {ctx.UserId}");
        Console.WriteLine($"Tenant: {ctx.TenantId}");
        ctx.DoSensitiveOperation();
    }
}

// Untrusted service - read-only access
public class UntrustedService(IContextAccessor<IReadOnlyContext> context)
{
    public void Execute()
    {
        var ctx = context.RequireCurrent();
        
        // ✅ Limited access
        Console.WriteLine($"Context: {ctx.Id}");
        Console.WriteLine($"Created: {ctx.CreatedAt}");
        
        // ❌ Cannot access sensitive data
        // Console.WriteLine(ctx.UserId);  // Compilation error
    }
}

// Usage
var manager = sp.GetRequiredService<IContextManager<MyContext>>();
var trusted = sp.GetRequiredService<TrustedService>();
var untrusted = sp.GetRequiredService<UntrustedService>();

var context = MyContext.Create("user-123", "tenant-456");

await manager.ExecuteInContext(context, async () =>
{
    trusted.DoWork();      // Full access
    untrusted.Execute();   // Read-only access
    await Task.CompletedTask;
});
```

---

## See Also

- [Getting Started](Getting-Started.md) - Setup and basic usage
- [Core Concepts](Core-Concepts.md) - Understanding the architecture
- [Best Practices](Best-Practices.md) - Tips and recommendations
---

> **Built with ❤️ for .NET developers**