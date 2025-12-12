# Basic Usage

This guide demonstrates the core patterns for using the Context System in your .NET applications, including creating, accessing, and executing code within a context.

## 1. Creating a Context

Define a context class implementing `IContext`:

```csharp
using Core.Features.Context.Abstractions;

public class MyAppContext(string userId, string tenantId) : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public string UserId { get; } = userId;
    public string TenantId { get; } = tenantId;
    public string CorrelationId { get; } = Guid.NewGuid().ToString();
}
```
**Notes:**
* ID and CreatedAt are mandatory for all contexts
* Add any additional properties required for your application
* Keep contexts immutable for thread safety
 
## 2. Registering Context in Dependency Injection
```csharp
var services = new ServiceCollection();

// Registers context infrastructure
services.AddContext<MyAppContext>();
services.AddReadOnlyContextAccessor<MyAppContext>();

var sp = services.BuildServiceProvider();
```

**What gets registered:**
* `AddContext<TContext>()` **registers**:
* * `ContextStore<TContext>`
* * `IContextAccessor<TContext>`
* * `IContextManager<TContext>`
* `AddReadOnlyContextAccessor<TContext>()` **registers**:
* * `IContextAccessor<IReadOnlyContext>` for untrusted code

## 3. Accessing Current Context
Inject `IContextAccessor<TContext>` in your services:

```csharp
public class OrderService(IContextAccessor<MyAppContext> contextAccessor)
{
    public void ProcessOrder()
    {
        var ctx = contextAccessor.RequireCurrent();
        Console.WriteLine($"Processing order for user: {ctx.UserId}, tenant: {ctx.TenantId}");
    }
}
```

**Available methods:**

| Method             | Behavior                                        |
|--------------------|-------------------------------------------------|
| `RequireCurrent()` | Returns context; throws if no context is active |
| `TryGetCurrent()`  | Returns context or `null`                       |
| `GetCurrent()`     | Returns context or `null`; no exception         |
| `HasContext()`     | Returns `true` if context is active             |

## 4. Executing Code Within Context
```csharp
var manager = sp.GetRequiredService<IContextManager<MyAppContext>>();
var service = sp.GetRequiredService<OrderService>();

var context = new MyAppContext("user-123", "tenant-456");

await manager.ExecuteInContext(context, async () =>
{
    service.ProcessOrder(); // Context flows automatically
    await Task.Delay(10);   // Context remains available in async calls
});
```
**Key features:**
* `ExecuteInContext` sets the context for the duration of the action
* Context is automatically restored/cleaned up afterward
* Works seamlessly with async and multithreaded code
* Supports nesting and context switching

## 5. Using Read-Only Context in Untrusted Code
```csharp
public class AuditService(IContextAccessor<IReadOnlyContext> context)
{
    public void Log()
    {
        var ctx = context.RequireCurrent();
        Console.WriteLine($"[ReadOnly] Context ID: {ctx.Id}, CreatedAt: {ctx.CreatedAt}");
    }
}
```
**Security benefits:**
* Read-only accessor ensures untrusted code cannot modify context
* Only `Id` and `CreatedAt` are visible
* Hides all custom application properties (UserId, TenantId, etc.)

## 6. Complete Example
```csharp
// 1. Define context
public class UserContext(string userId) : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public string UserId { get; } = userId;
    public string RequestId { get; } = Guid.NewGuid().ToString();
}

// 2. Register in DI
var services = new ServiceCollection();
services.AddContext<UserContext>();
services.AddReadOnlyContextAccessor<UserContext>();

// 3. Create service
public class UserService(IContextAccessor<UserContext> context)
{
    public string GetCurrentUserId()
    {
        return context.RequireCurrent().UserId;
    }
}

// 4. Execute with context
var provider = services.BuildServiceProvider();
var manager = provider.GetRequiredService<IContextManager<UserContext>>();
var userService = provider.GetRequiredService<UserService>();

await manager.ExecuteInContext(new UserContext("alice"), () =>
{
    var userId = userService.GetCurrentUserId(); // Returns "alice"
    Console.WriteLine($"Current user: {userId}");
});
```

## 7. Summary
1. Define a context class implementing `IContext`
2. Register context infrastructure in DI using `AddContext` and `AddReadOnlyContextAccessor`
3. Access context in services with:
   * `IContextAccessor<TContext>` for trusted code
   * `IContextAccessor<IReadOnlyContext>` for untrusted code
4. Use ExecuteInContext to scope context for operations and async flows
5. Read-only snapshots prevent untrusted code from modifying sensitive data

## Next Steps
- [Core Concepts](Core-Concepts.md) - Understanding the architecture

---

> **Built with ❤️ for .NET developers**