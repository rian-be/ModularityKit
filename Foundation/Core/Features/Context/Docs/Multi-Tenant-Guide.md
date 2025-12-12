# Multi-Tenant Guide

This guide explains how to handle multi-tenant environments in applications using the Context System. It demonstrates how to isolate data and context per tenant, propagate tenant IDs, and ensure security and consistency in asynchronous and multithreaded scenarios.

## 1. Introduction

Multi-tenancy allows a single application instance to serve multiple clients (tenants) while keeping their data and configuration isolated.

The Context System supports multi-tenancy by:

- Storing TenantId within the context (IContext / MyContext)
- Ensuring context isolation across async/await and threads using ContextStore
- Allowing read-only snapshots for untrusted or third-party components

## 2. Tenant ID in Context

Every application context should include a tenant identifier:

```csharp
public class MyAppContext(string id, string userId, string tenantId, string correlationId)
    : IContext
{
    public string Id { get; } = id;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public string UserId { get; } = userId;
    public string TenantId { get; } = tenantId;
    public string CorrelationId { get; } = correlationId;

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

## 3. Register Context in DI
```csharp
var services = new ServiceCollection();

// Register full context and read-only accessor
services.AddContext<MyAppContext>();
services.AddReadOnlyContextAccessor<MyAppContext>();

var serviceProvider = services.BuildServiceProvider();
```

##  4. Using Tenant-Aware Services
Tenant-aware services automatically retrieve tenant information from the current context:
```csharp
public class DocumentService
{
    private readonly IContextAccessor<MyAppContext> _context;

    public DocumentService(IContextAccessor<MyAppContext> context)
    {
        _context = context;
    }

    public async Task<Document?> GetDocumentAsync(string documentId)
    {
        var ctx = _context.RequireCurrent();

        // Automatic tenant filtering
        return await _db.Documents
            .Where(d => d.TenantId == ctx.TenantId)
            .Where(d => d.Id == documentId)
            .FirstOrDefaultAsync();
    }
}
```

## 5. Executing Code in a Tenant Context
```csharp
var contextManager = serviceProvider.GetRequiredService<IContextManager<MyAppContext>>();
var documentService = serviceProvider.GetRequiredService<DocumentService>();

var tenantContext = MyAppContext.Create("user-123", "tenant-456");

await contextManager.ExecuteInContext(tenantContext, async () =>
{
    var document = await documentService.GetDocumentAsync("doc-001");
    Console.WriteLine($"Tenant {tenantContext.TenantId} accessed document: {document?.Id}");
});
```

**Key points:**
* ExecuteInContext ensures that all code executed inside the lambda sees the correct tenant
* Context is automatically restored after execution
* All nested calls automatically inherit the tenant context

## 6. Security Best Practices
* **Never pass the full context (`IContext`) to untrusted or third-party code** — use `IReadOnlyContext`
* **Treat** `TenantId` as immutable within context
* **Always use** `RequireCurrent()` to ensure a context is active before accessing tenant-specific data
* **Avoid storing context in static fields** to prevent cross-tenant leaks
* **Validate tenant access** in authorization middleware before setting context

## 7. Example: Tenant Isolation Pattern
```csharp
public class TenantScopedRepository<T>(
    IContextAccessor<MyAppContext> contextAccessor,
    DbContext dbContext)
    where T : ITenantEntity
{

    public IQueryable<T> GetTenantQueryable()
    {
        var context = contextAccessor.RequireCurrent();
        return dbContext.Set<T>().Where(x => x.TenantId == context.TenantId);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await GetTenantQueryable().FirstOrDefaultAsync(x => x.Id == id);
    }
}

public interface ITenantEntity
{
    string TenantId { get; set; }
}
```

## 8. Advanced Scenarios
### Nested Tenant Contexts
```csharp
await contextManager.ExecuteInContext(parentContext, async () =>
{
    // Switch to different tenant for specific operation
    var tempContext = new MyAppContext("system", "tenant-b");
    await contextManager.ExecuteInContext(tempContext, async () =>
    {
        // Perform cross-tenant operation
    });
    // Automatically returns to parent tenant context
});
```
### Tenant-Aware Background Processing
```csharp
public class TenantAwareBackgroundService(IContextManager<MyAppContext> contextManager,
    IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var jobs = await GetPendingJobsAsync();
            
            foreach (var job in jobs)
            {
                var context = MyAppContext.Create(job.UserId, job.TenantId);
                
                await contextManager.ExecuteInContext(context, async () =>
                {
                    await ProcessJobAsync(job);
                });
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

## 9. Summary
The Context System provides robust multi-tenancy support through:

* **TenantId in context** — enables automatic tenant isolation
* **ContextStore** — ensures thread-safe and async-safe propagation
* **Read-only snapshots** — secure access for untrusted code paths
* **Tenant-aware services** — automatic filtering based on current context
* **Deterministic lifecycle** — prevents cross-tenant contamination

By following this guide, you can build secure, isolated multi-tenant applications that properly separate tenant data while maintaining clean architecture.

---

> **Built with ❤️ for .NET developers**