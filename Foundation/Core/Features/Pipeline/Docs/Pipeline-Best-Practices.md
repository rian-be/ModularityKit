# Pipeline System - Best Practices

Recommended patterns, constraints, and conventions for using the Pipeline System effectively in production grade.

---

## Table of Contents

- [Core Principles](#core-principles)
- [Middleware Design](#middleware-design)
- [Conditional Execution](#conditional-execution)
- [Branch Pipelines](#branch-pipelines)
- [Performance Optimization](#performance-optimization)
- [Diagnostics & Debugging](#diagnostics--debugging)
- [Testing Strategies](#testing-strategies)
- [Common Pitfalls](#common-pitfalls)

---

## Core Principles

### 1. Keep Middleware Single-Purpose

**Recommendation:** Each middleware should perform a single, well-defined task.

**Benefits:**
- ✅ Easier to test and debug
- ✅ Clear execution order
- ✅ Reusable across pipelines
- ✅ Better maintainability

**✅ Good:**
```csharp
var builder = new PipelineBuilder<RequestContext>();

// Each middleware has one responsibility
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());
builder.Use(new AuthenticationMiddleware());
builder.Use(new AuthorizationMiddleware());
```

**❌ Avoid:**
```csharp
// Combines logging, validation, and authentication in one middleware
public class MegaMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ❌ Too many responsibilities
        Log(context);
        Validate(context);
        Authenticate(context);
        Authorize(context);
        
        await next();
    }
}
```

---

### 2. Keep Middleware Stateless

Middleware should not maintain internal mutable state across requests.

**✅ Good:**
```csharp
public class LoggingMiddleware : IMiddleware<RequestContext>
{
    private readonly ILogger _logger;  // ✅ Stateless dependency
    
    public LoggingMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task InvokeAsync(
        RequestContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing request for {UserId}", context.UserId);
        await next();
    }
}
```

**❌ Avoid:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    private int _counter = 0;  // ❌ Mutable state - race condition!
    
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        _counter++;  // ❌ Not thread-safe
        Console.WriteLine($"Request #{_counter}");
        await next();
    }
}
```

**Why:**
- Middleware instances are typically shared across requests
- Mutable state causes race conditions
- Makes testing difficult
- Unpredictable behavior in concurrent scenarios

---

### 3. Use Meaningful Contexts

Pipeline context (`TContext`) should be small, focused, and immutable.

**✅ Good:**
```csharp
public sealed class RequestContext
{
    // Metadata for branching and logging
    public string UserId { get; init; }
    public string TenantId { get; init; }
    public string[] Roles { get; init; }
    public Dictionary<string, object> Metadata { get; init; }
}
```

**❌ Avoid:**
```csharp
public class BadContext
{
    // ❌ Too much data
    public DbContext Database { get; set; }
    public HttpClient HttpClient { get; set; }
    public byte[] LargePayload { get; set; }
    
    // ❌ Business objects don't belong here
    public Order CurrentOrder { get; set; }
    public ShoppingCart Cart { get; set; }
    
    // ❌ Sensitive data
    public string ApiKey { get; set; }
    public string JwtToken { get; set; }
}
```

**Guidelines:**
- ✅ Include metadata needed for branching and conditional logic
- ✅ Keep contexts immutable
- ✅ Store identifiers, not entire objects
- ❌ Don't pass large domain objects
- ❌ Don't store sensitive secrets
- ❌ Don't store stateful services

---

## Middleware Design

### 4. Avoid Heavy Operations Inside Middleware

Middleware should be lightweight. Offload heavy operations to services.

**✅ Good:**
```csharp
public class ValidationMiddleware : IMiddleware<RequestContext>
{
    private readonly IValidator _validator;
    
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ✅ Lightweight validation
        if (!await _validator.IsValidAsync(context))
        {
            throw new ValidationException();
        }
        
        await next();
    }
}
```

**❌ Avoid:**
```csharp
public class HeavyMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ❌ Heavy database query
        var data = await _db.Users
            .Include(u => u.Orders)
            .Include(u => u.Preferences)
            .Include(u => u.History)
            .ToListAsync();
        
        // ❌ Large file I/O
        var file = await File.ReadAllBytesAsync("large-file.dat");
        
        // ❌ CPU-bound calculation
        var result = PerformComplexCalculation(data);
        
        await next();
    }
}
```

**Good Operations:**
- ✅ Logging to memory or async logger
- ✅ Validation checks
- ✅ Minor transformations
- ✅ Setting metadata

**Avoid:**
- ❌ Synchronous database queries
- ❌ Large file I/O
- ❌ Complex calculations
- ❌ External API calls in hot path

---

### 5. Use Dependency Injection

Inject dependencies through constructor, not via context.

**✅ Good:**
```csharp
public class AuthenticationMiddleware : IMiddleware<RequestContext>
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;
    
    public AuthenticationMiddleware(IAuthService authService, ILogger logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        var user = await _authService.AuthenticateAsync(context.UserId);
        await next();
    }
}
```

**❌ Avoid:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(BadContext context, Func<Task> next, ...)
    {
        // ❌ Using context as service locator
        var db = context.Database;
        var client = context.HttpClient;
        
        await next();
    }
}
```

---

## Conditional Execution

### 6. Prefer UseWhen for Conditional Execution

Conditional logic should reside in the pipeline builder, not inside middleware.

**✅ Good:**
```csharp
var builder = new PipelineBuilder<RequestContext>();

builder.Use(new LoggingMiddleware());

// ✅ Declarative conditional execution
builder.UseWhen(
    ctx => ctx.Roles.Contains("Admin"),
    new AdminAuditMiddleware()
);

// ✅ Multiple conditions
builder.UseWhen(
    ctx => ctx.Metadata["Plan"] as string == "Premium",
    new PremiumFeaturesMiddleware()
);

builder.Use(new ProcessingMiddleware());
```

**❌ Avoid:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ❌ Internal branching
        if (context.Roles.Contains("Admin"))
        {
            await DoAdminStuff(context);
        }
        
        if (context.Metadata["Plan"] as string == "Premium")
        {
            await DoPremiumStuff(context);
        }
        
        await next();
    }
}
```

**Benefits:**
- ✅ Declarative and readable
- ✅ Easier to reorder or test
- ✅ Avoids scattered `if` statements
- ✅ Better separation of concerns

---

### 7. Keep Predicates Simple and Deterministic

Conditional predicates should be simple and free of side effects.

**✅ Good:**
```csharp
// Simple, deterministic predicates
builder.UseWhen(ctx => ctx.Roles.Contains("Admin"), middleware);
builder.UseWhen(ctx => ctx.TenantId.StartsWith("premium-"), middleware);
builder.UseWhen(ctx => ctx.Metadata.ContainsKey("Debug"), middleware);
```

**❌ Avoid:**
```csharp
// ❌ Complex, non-deterministic predicates
builder.UseWhen(ctx =>
{
    // ❌ Database call in predicate
    var user = _db.Users.Find(ctx.UserId);
    return user.IsPremium;
}, middleware);

// ❌ Random behavior
builder.UseWhen(ctx => Random.Shared.Next(100) > 50, middleware);

// ❌ Side effects
builder.UseWhen(ctx =>
{
    Console.WriteLine("Checking condition...");  // ❌ Side effect
    return ctx.Roles.Contains("Admin");
}, middleware);
```

---

## Branch Pipelines

### 8. Use Branch Pipelines for Isolated Scenarios

Branches allow you to isolate tenant-specific, role-specific, or feature-specific pipelines.

**✅ Good:**
```csharp
var builder = new PipelineBuilder<TenantContext>();

builder.Use(new LoggingMiddleware());

// ✅ Isolated enterprise features
builder.UseBranch(
    ctx => ctx.Plan == "Enterprise",
    enterpriseBranch =>
    {
        enterpriseBranch.Use(new EnterpriseFeaturesMiddleware());
        enterpriseBranch.Use(new DedicatedSupportMiddleware());
        enterpriseBranch.Use(new CustomIntegrationMiddleware());
    }
);

// ✅ Isolated free tier limitations
builder.UseBranch(
    ctx => ctx.Plan == "Free",
    freeBranch =>
    {
        freeBranch.Use(new RateLimitingMiddleware());
        freeBranch.Use(new FeatureLimitMiddleware());
    }
);

builder.Use(new CommonProcessingMiddleware());
```

**Guidelines:**
- ✅ Keep branch predicates simple and deterministic
- ✅ Avoid side effects outside the branch
- ✅ Nested branches are allowed but keep depth reasonable (max 2-3 levels)
- ✅ Each branch should be independently testable

**❌ Avoid:**
```csharp
// ❌ Too deeply nested
builder.UseBranch(ctx => condition1, b1 =>
{
    b1.UseBranch(ctx => condition2, b2 =>
    {
        b2.UseBranch(ctx => condition3, b3 =>
        {
            b3.UseBranch(ctx => condition4, b4 =>  // ❌ Too deep!
            {
                // ...
            });
        });
    });
});
```

---

## Performance Optimization

### 9. Do Not Modify Pipeline During Execution

Pipeline structure should be stable during execution.

**✅ Good:**
```csharp
// Build pipeline once
var builder = new PipelineBuilder<RequestContext>();
builder.Use(new LoggingMiddleware());
builder.Use(new ValidationMiddleware());

var executor = new PipelineExecutor<RequestContext>(builder);

// Execute many times
for (int i = 0; i < 1000; i++)
{
    await executor.ExecuteAsync(context);
}
```

**❌ Avoid:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ❌ Modifying pipeline during execution
        if (context.Metadata["AddMiddleware"] as bool? == true)
        {
            _builder.Use(new DynamicMiddleware());
        }
        
        await next();
    }
}
```

**Risks:**
- ❌ Unexpected behavior if pipeline changes mid-flight
- ❌ Diagnostic inconsistencies
- ❌ Hard-to-reproduce bugs
- ❌ Thread-safety issues

**Dynamic Modification:**

If you need to modify the pipeline, use `PipelineInspector` **before** execution:
```csharp
var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// Modify before execution
if (shouldAddMiddleware)
{
    builder.Use(new DynamicMiddleware());
}

// Rebuild executor
var executor = new PipelineExecutor<RequestContext>(builder);
await executor.ExecuteAsync(context);
```

---

### 10. Avoid Unnecessary Allocations

**✅ Good:**
```csharp
public class EfficientMiddleware : IMiddleware<RequestContext>
{
    private static readonly string[] EmptyRoles = Array.Empty<string>();
    
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ✅ Reuse static empty array
        var roles = context.Roles ?? EmptyRoles;
        
        await next();
    }
}
```

**❌ Avoid:**
```csharp
public class WastefulMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        // ❌ Unnecessary allocations
        var list = new List<string>();
        var dict = new Dictionary<string, object>();
        var array = new string[100];
        
        await next();
    }
}
```

---

## Diagnostics & Debugging

### 11. Enable Debug Instrumentation Only in Development

`PipelineDebugScope` has zero-cost when disabled, but enabling it in production may leak sensitive metadata.

**✅ Good:**
```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    using var scope = PipelineDebugScope.Begin(out var debug);
    await executor.ExecuteAsync(context);
    
    // Log diagnostics
    foreach (var step in debug.Steps)
    {
        _logger.LogDebug(
            "{Middleware} executed in {Duration}ms",
            step.Middleware.GetType().Name,
            step.Duration?.TotalMilliseconds
        );
    }
}
else
{
    // ✅ Zero overhead in production
    await executor.ExecuteAsync(context);
}
```

**❌ Avoid:**
```csharp
// ❌ Always enabled - performance overhead in production
using var scope = PipelineDebugScope.Begin(out var debug);
await executor.ExecuteAsync(context);
```

**Recommendation:**
- ✅ Use debug scope in development or testing
- ✅ Use feature flags to control diagnostics
- ❌ Avoid enabling in high-throughput production scenarios unless needed

---

### 12. Use PipelineInspector for Runtime Analysis

Use `PipelineInspector<TContext>` to analyze pipeline structure.

**✅ Good:**
```csharp
var inspector = new PipelineInspector<RequestContext>(builder.Middlewares);

// List middlewares
Console.WriteLine("Pipeline structure:");
foreach (var mw in inspector.GetMiddlewares())
{
    Console.WriteLine($"  - {mw.GetType().Name}");
}

// Get metadata
foreach (var desc in inspector.GetDescriptors())
{
    Console.WriteLine(
        $"{desc.Name} [{desc.Kind}] Conditional={desc.IsConditional}"
    );
}

// Remove middleware before execution
inspector.Remove(mw => mw is DebugMiddleware);

// Rebuild executor
var executor = new PipelineExecutor<RequestContext>(builder);
```

**Guidelines:**
- ✅ Do not modify the pipeline during active execution
- ✅ Use thread-safe inspection patterns
- ✅ Combine with diagnostics for testing or dev-time analysis

---

## Testing Strategies

### 13. Test Pipelines in Isolation

Test each pipeline branch and middleware combination.

**✅ Good:**
```csharp
[Fact]
public async Task Pipeline_AdminUser_ExecutesAuditMiddleware()
{
    // Arrange
    var builder = new PipelineBuilder<TestContext>();
    builder.Use(new LoggingMiddleware());
    builder.UseWhen(
        ctx => ctx.IsAdmin,
        new AdminAuditMiddleware()
    );
    
    var executor = new PipelineExecutor<TestContext>(builder);
    var context = new TestContext { IsAdmin = true };
    
    // Act
    using var scope = PipelineDebugScope.Begin(out var debug);
    await executor.ExecuteAsync(context);
    
    // Assert
    Assert.Contains(
        debug.Steps,
        s => s.Middleware is AdminAuditMiddleware
    );
}

[Fact]
public async Task Pipeline_RegularUser_SkipsAuditMiddleware()
{
    // Arrange
    var builder = new PipelineBuilder<TestContext>();
    builder.Use(new LoggingMiddleware());
    builder.UseWhen(
        ctx => ctx.IsAdmin,
        new AdminAuditMiddleware()
    );
    
    var executor = new PipelineExecutor<TestContext>(builder);
    var context = new TestContext { IsAdmin = false };
    
    // Act
    using var scope = PipelineDebugScope.Begin(out var debug);
    await executor.ExecuteAsync(context);
    
    // Assert
    Assert.DoesNotContain(
        debug.Steps,
        s => s.Middleware is AdminAuditMiddleware
    );
}
```

**Verify:**
- ✅ Conditional branches execute correctly
- ✅ Execution order is correct
- ✅ `next()` calls happen as expected
- ✅ Each middleware performs its task

---

### 14. Mock Dependencies in Middleware Tests

**✅ Good:**
```csharp
[Fact]
public async Task AuthenticationMiddleware_ValidUser_CallsNext()
{
    // Arrange
    var mockAuthService = new Mock<IAuthService>();
    mockAuthService
        .Setup(x => x.AuthenticateAsync(It.IsAny<string>()))
        .ReturnsAsync(new User { Id = "user-123" });
    
    var middleware = new AuthenticationMiddleware(mockAuthService.Object);
    var context = new RequestContext { UserId = "user-123" };
    var nextCalled = false;
    
    // Act
    await middleware.InvokeAsync(context, () =>
    {
        nextCalled = true;
        return Task.CompletedTask;
    });
    
    // Assert
    Assert.True(nextCalled);
    mockAuthService.Verify(x => x.AuthenticateAsync("user-123"), Times.Once);
}
```

---

## Common Pitfalls

### Pitfall 1: Not Calling next()

**Problem:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        Console.WriteLine("Processing...");
        // ❌ Forgot to call next() - pipeline stops here!
    }
}
```

**Solution:**
```csharp
public class GoodMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        Console.WriteLine("Before");
        await next();  // ✅ Always call next()
        Console.WriteLine("After");
    }
}
```

---

### Pitfall 2: Using Console.WriteLine

**Problem:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        Console.WriteLine($"User: {context.UserId}");  // ❌ Don't use Console
        await next();
    }
}
```

**Solution:**
```csharp
public class GoodMiddleware : IMiddleware<RequestContext>
{
    private readonly ILogger _logger;
    
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        _logger.LogInformation("Processing for {UserId}", context.UserId);  // ✅ Use ILogger
        await next();
    }
}
```

---

### Pitfall 3: Catching Exceptions Without Re-throwing

**Problem:**
```csharp
public class BadMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);  // ❌ Swallowed exception
        }
    }
}
```

**Solution:**
```csharp
public class GoodMiddleware : IMiddleware<RequestContext>
{
    public async Task InvokeAsync(RequestContext context, Func<Task> next, ...)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            throw;  // ✅ Re-throw to propagate
        }
    }
}
```

---

## Summary

Following these best practices ensures:

- ✅ Predictable, async-safe execution
- ✅ Declarative, testable pipelines
- ✅ Clear separation of responsibilities
- ✅ Minimal runtime overhead
- ✅ Safe conditional and branch logic

**Pipeline System works best when:**

- Middleware is stateless and single-purpose
- Conditional logic is handled via `UseWhen`
- Branch pipelines isolate optional or tenant-specific logic
- Debug instrumentation is enabled only in dev/test
- Pipelines are stable during execution

---

## See Also

- [Getting Started](Pipeline-Getting-Started.md) - Setup and basic usage
- [Basic Usage](Pipeline-Basic-Usage.md) - Common patterns
- [Pipeline Guide](Pipeline-Guide.md) - Complete development guide
- [API Reference](Pipeline-API-Reference.md) - Complete API documentation

---

> **Built with ❤️ for .NET developers**