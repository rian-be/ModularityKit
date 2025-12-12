# Getting Started

This guide will walk you through setting up and using the Context System in your .NET application.

## Prerequisites

- .NET 9.0 or later
- Basic understanding of dependency injection
- Familiarity with async/await

## Installation

### 1. Add Project Reference
```bash
dotnet add reference Core/Core.csproj
```

### 2. Add Using Statements
```csharp
using Core.Features.Context.Abstractions;
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;
```

## Basic Setup

### Step 1: Define Your Context

Create a class that implements `IContext`:
```csharp
using Core.Features.Context.Abstractions;

public class MyAppContext : IContext
{
    // Required by IContext
    public string Id { get; }
    public DateTimeOffset CreatedAt { get; }
    
    // Your custom properties
    public string UserId { get; }
    public string TenantId { get; }
    public string CorrelationId { get; }
    
    public MyAppContext(
        string id,
        string userId,
        string tenantId,
        string correlationId)
    {
        Id = id;
        UserId = userId;
        TenantId = tenantId;
        CorrelationId = correlationId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    // Factory method for convenience
    public static MyAppContext Create(string userId, string tenantId)
    {
        return new MyAppContext(
            id: Guid.NewGuid().ToString(),
            userId: userId,
            tenantId: tenantId,
            correlationId: Guid.NewGuid().ToString()
        );
    }
}
```

### Step 2: Register in Dependency Injection
```csharp
var services = new ServiceCollection();

// Register context infrastructure
services.AddContext<MyAppContext>();
services.AddReadOnlyContextAccessor<MyAppContext>();

var serviceProvider = services.BuildServiceProvider();
```

**What this registers:**
- `ContextStore<MyAppContext>` - Internal storage
- `IContextAccessor<MyAppContext>` - Access current context
- `IContextManager<MyAppContext>` - Manage context lifecycle

### Step 3: Inject and Use
```csharp
public class OrderService(IContextAccessor<MyAppContext> contextAccessor)
{
    public async Task CreateOrder(OrderRequest request)
    {
        // Get current context
        var ctx = contextAccessor.RequireCurrent();
        
        // Use context data
        var order = new Order
        {
            UserId = ctx.UserId,
            TenantId = ctx.TenantId,
            CorrelationId = ctx.CorrelationId,
            // ... other properties
        };
        
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync();
    }
}
```

### Step 4: Execute with Context
```csharp
// Get services
var contextManager = serviceProvider.GetRequiredService<IContextManager<MyAppContext>>();
var orderService = serviceProvider.GetRequiredService<OrderService>();

// Create context
var context = MyAppContext.Create("user-123", "tenant-456");

// Execute with context active
await contextManager.ExecuteInContext(context, async () =>
{
    await orderService.CreateOrder(new OrderRequest());
    // Context is active for all operations here
});
// Context is automatically cleaned up
```

## Complete Example

Here's a complete console application:
```csharp
using Core.Features.Context.Abstractions;
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Define context
public class MyContext(string id, string userId) : IContext
{
    public string Id { get; } = id;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public string UserId { get; } = userId;
}

// Define service
public class GreetingService(IContextAccessor<MyContext> context)
{
    public void SayHello()
    {
        var ctx = context.RequireCurrent();
        Console.WriteLine($"Hello, {ctx.UserId}!");
    }
}

// Setup and run
var services = new ServiceCollection();
services.AddContext<MyContext>();
services.AddReadOnlyContextAccessor<MyContext>();
services.AddTransient<GreetingService>();

var sp = services.BuildServiceProvider();

var manager = sp.GetRequiredService<IContextManager<MyContext>>();
var greeting = sp.GetRequiredService<GreetingService>();


var context = new MyContext("ctx-1", "Alice");

await manager.ExecuteInContext(context, async () =>
{
    greeting.SayHello();  // Output: Hello, Alice!
    await Task.CompletedTask;
});
```

## Next Steps

- **[Basic Usage](Basic-Usage.md)** - Learn more patterns
- **[Core Concepts](Core-Concepts.md)** - Understand how it works
- **[API Reference](API-Reference.md)** - Full API documentation

---

> **Built with ❤️ for .NET developers**