# Best Practices

This document describes recommended patterns, constraints, and conventions for using the Context System effectively in production-grade .NET applications. Following these guidelines ensures predictable behavior, optimal performance, and safe usage across asynchronous execution flows.

## 1. Keep Context Immutable

**Recommendation:**
Define your context types as fully immutable, with all state provided via constructor parameters.

**Benefits:**
- Thread-safety
- Predictable behavior across async boundaries
- No risk of mid-execution mutation
- Simpler debugging and inspection
- Safe sharing across concurrent operations

**Example (good):**
```csharp
public class MyContext : IContext
{
    public string Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public string UserId { get; }

    public MyContext(string id, string userId)
    {
        Id = id;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
```

**Example (avoid):**
```csharp
public string UserId { get; set; }   // mutable ❌
```

## 2. Create Contexts Explicitly
Contexts should not be created implicitly inside services or components.

**Good:**
* Context constructed at entry point
* Propagated through ExecuteInContext
* Lifecycle boundaries clearly defined

**Avoid:**
* Creating new contexts inside services
* Implicit or hidden construction
* Mixing unrelated scopes

This maintains traceability and eliminates "ghost" context values.

## 3. Use ExecuteInContext for Every Context Activation
Never set or mutate the context manually. Always rely on:
```csharp
await contextManager.ExecuteInContext(context, async () =>
{
    // code with active context
});
```

**Reasons:**
* Ensures correct cleanup
* Avoids context leaks
* Preserves previous context in nested scopes
* Guarantees async safety
* Centralizes lifecycle logic

Manual manipulation of the store is considered unsafe.

## 4. Keep Contexts Lightweight
Context objects should represent metadata about execution, not heavy domain objects.

**Recommended properties:**
* Identifiers (UserId, TenantId, RequestId, CorrelationId)
* Timestamps
* Lightweight flags or modes

**Avoid storing:**
* EF DbContexts
* Open connections
* Large payloads
* Business objects
* Complex domain states

Contexts should be portable, serializable, and cheap to construct.

## 5. Use Read-Only Context for Untrusted or External Modules
If the application supports:
* Plugin systems
* Modular architecture
* Sandboxed user logic
* Dynamic extensions
* Third-party workflows

Then register:
```csharp
services.AddReadOnlyContextAccessor<MyContext>();
```
This protects sensitive metadata and prevents untrusted code from:
* Mutating context
* Inspecting private application-level fields
* Relying on internal implementation details

Read-only access is a core part of the security model.

## 6. Do Not Share Context Instances Across Unrelated Executions
Each logical operation should receive its own context instance.

**Example (good):**
```csharp
var context = MyContext.Create(userId, tenantId);
await manager.ExecuteInContext(context, ProcessOrder);
```
**Example (avoid):**
```csharp
// Reusing the same instance across multiple requests ❌
_globalContext = existingContext;
```

Reuse creates:
* Cross-request contamination
* Data races
* Deeply confusing logs

## 7. Avoid Async Fire-and-Forget Inside Context Scopes
Do not create background tasks inside a context scope:
```csharp
Task.Run(() => DoSomething());  // risky ❌
```

**Why:**
* Background work may capture the wrong context
* Cleanup on the original scope may run before the task starts
* Diagnostic behavior becomes non-deterministic

**If you must run background work:**
* Capture the context explicitly
* Isolate execution using a new context instance
* Activate it inside its own **ExecuteInContext**

## 8. Never Expose ContextStore<TContext> Publicly
`ContextStore<TContext>` is a low-level infrastructure primitive.

### Avoid injecting it into services.
Authorized entry points:
* `IContextAccessor<TContext>` for reading
* `IContextManager<TContext>` for scoped execution

This prevents store misuse and enforces correct lifecycle boundaries.

## 9. Enforce Context Early (Entry Points)
Best practice is to activate context at:
* API request boundaries
* Message bus handlers
* Background worker jobs
* Hosted service loop iterations
* CLI command handlers
* Integration event handlers

>Do not defer context activation until deep inside the call graph.
>> This ensures consistent correlation and observability from the start.

## 10. Use Context for Correlation, Not Authorization
Contexts are metadata containers. They should not replace:

* Authorization policies
* Claims identities
* Permission models

**Correct usage:**
* Storing TenantId for scoping logs
* Storing UserId for audit trails
* Storing RequestId for tracing

**Incorrect usage:**
* Enforcing access control purely through context values
* Implementing permission checks in domain services based only on context

Use context to enrich flows, not as a security mechanism.

## 11. Use Meaningful, Unique Context Ids
ID should uniquely identify the logical execution flow.

**Examples:**
* GUID (recommended)
* Request ID
* Correlation ID

The ID is especially important for:
* Distributed tracing
* Logs
* Debugging async call stacks
* Post-mortem analysis

## 12. Do Not Store Sensitive Secrets Inside Context
Contexts flow across many layers and async boundaries.

**They should not contain:**
* Raw JWT tokens
* API keys
* Passwords
* OAuth credentials
* Session cookies
* Unencrypted sensitive data

**Instead:**
* Store identifiers
* Resolve sensitive data via proper credential stores

## 13. Use Wrappers for Cross-Cutting Infrastructure
Context is often used with:

* Logging scopes
* Telemetry
* Message correlation
* Domain events
* Workflow engines
* Sagas

**The recommended pattern is wrapper:**
```csharp
public async Task ExecuteWithContextAsync(MyContext context, Func<Task> work)
{
    await _contextManager.ExecuteInContext(context, async () =>
    {
        using (_logger.BeginScope(new { context.Id, context.UserId }))
        {
            await work();
        }
    });
}
```

## 14. Keep Context Types Focused
Do not overload a single context with everything:

**Bad:**
```csharp
public class GiantContext : IContext
{
    public UserDto UserProfile { get; set; }
    public ShoppingCart Cart { get; set; }
    public FeatureFlags Flags { get; set; }
    // ... bloated
}
```
**Good:**
* One context per execution boundary
* Small set of core metadata
* Reflect only what your system actually needs to correlate and identify

## 15. Use Context in Tests
Testing with the context system is straightforward:
```csharp
var context = new MyContext("test", "test-user");
await manager.ExecuteInContext(context, () => service.DoWork())
```
This ensures test logic matches production behavior exactly.

Avoid manipulating the store directly.

## Summary
Following these best practices ensures:
* Predictable, async-safe behavior
* Clean separation between execution metadata and business logic
* Optimal observability and debugging
* Secure and safe interactions in modular environments
* Maintainable long-term architecture

The Context System is most effective when contexts are:
* Immutable
* Lightweight
* Explicitly created
* Correctly scoped
* Never mutated
* Isolated per execution flow

---

> **Built with ❤️ for .NET developers**