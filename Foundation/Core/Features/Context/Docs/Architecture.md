# Architecture

This document provides a comprehensive overview of the internal architecture of the Context System. The architecture is designed to provide deterministic, async-safe, and type-isolated contextual execution for .NET applications without relying on global static state or uncontrolled ambient data.

---

## Architectural Principles

The architecture follows four foundational principles.

### 1. Strong Isolation Per Context Type

Each `TContext` is fully isolated:

* Independent store
* Independent accessor
* Independent lifecycle
* No possibility of cross-type interference

This enables multiple context models to coexist in the same application without collisions.

### 2. Async-Safe Context Propagation

The system ensures:

* Context flows across await boundaries
* Concurrent async tasks have isolated state
* Context does not leak to unrelated execution flows

This is achieved without directly exposing `AsyncLocal<T>` to the rest of the system.

### 3. Deterministic Lifecycle Management

Context state is:

* Created explicitly
* Activated explicitly
* Confined to a controlled execution region
* Cleaned up deterministically

This eliminates the "ambient context leak" problem common in many .NET libraries.

### 4. Read/Write Separation

Two access modes exist:

* **Full context** for internal logic (`IContextAccessor<TContext>`)
* **Read-only** for untrusted logic (`IReadOnlyContext`)

This enforces security boundaries in architectures with plugins, multi-tenant logic, or user-defined modules.

---

## Component Architecture

### ContextStore<TContext>

`ContextStore<TContext>` is the lowest-level component responsible for holding the active context instance for a specific context type.

**Responsibilities:**

* Stores the active context
* Provides get/set/clear operations
* Ensures async-flow isolation

**It does not control:**

* Creation of the context
* Cleanup logic
* Lifecycle ordering

It is intentionally minimal.

---
### IContextAccessor<TContext>

The accessor is a safe façade around the store.

It provides:

* `TryGetCurrent()`
* `GetCurrent()`
* `RequireCurrent()`
* `HasContext()`

Service code depends on this interface to consume context, but never to mutate it.

This enforces a strict boundary: services cannot accidentally override or reset the context.

### IContextManager<TContext>

The manager governs the lifecycle:

```csharp
await manager.ExecuteInContext(context, async () =>
{
    // context is active
});
```

### **Internal process:**
1. Saves previous context (for stacking safety)
2. Sets the new context in the store
3. Executes the delegate
4. Restores the previous context after the delegate finishes
5. Guarantees cleanup even on failure

This is equivalent to a controlled, type-safe ambient scope.

## ReadOnlyContextAccessor<TContext>
This adapter protects against leaking internal metadata to untrusted or sandboxed code.

**Transformation:**

`TContext` → `IReadOnlyContext`

**Preserved data only:**
* Id
* CreatedAt

All custom application metadata is hidden. This provides a safe context boundary between trusted and non-trusted services.

---

## Dependency Injection Registration
```csharp
services.AddContext<MyContext>();
services.AddReadOnlyContextAccessor<MyContext>();
```

**DI Composition:**
* Singleton `ContextStore<MyContext>`
* Singleton `IContextAccessor<MyContext>`
* Singleton `IContextManager<MyContext>`
* Singleton `IContextAccessor<IReadOnlyContext>`

**Why singletons are correct:**
* Data is not stored in fields; stores use thread-local or async-flow storage
* Each store is isolated per context type
* Context is per-execution-flow, not per-instance

The architecture is entirely safe for multithreaded and multi-request environments.

### **Execution Macro-Flow**
```csharp
Application Code
      │ creates
      ▼
[Context Instance]
      │ passed into manager
      ▼
IContextManager<T>.ExecuteInContext
      │ sets context in store
      ▼
    Delegate
      │
      ▼
IContextAccessor<T> reads from store
      │
      ▼
Store maintains correct async-flow state
      │
      ▼
Manager restores previous context on completion
```

This defines a deterministic, well-encapsulated execution boundary.

### Why Not Just Use AsyncLocal?
The architecture intentionally avoids exposing AsyncLocal<T> because:

* AsyncLocal leaks across parallel continuations if misused
* Context switching requires manual stack management
* Type isolation is not automatic
* Resetting and restoring becomes error-prone
* Plugin/external modules should not see mutable data

**Instead:**
* Only the store uses AsyncLocal internally (implementation detail)
* All other components interact through safe APIs
* Lifecycle is controlled by a single place: the manager

This prevents the most common pitfalls of ambient context designs.

### Why Not Use HttpContext?
**Reasons:**
* Context should work in console apps, background workers, message buses, and hosted services
* Context should not require `ASP.NET Core` pipeline
* Context must be strongly typed
* `HttpContext` is request-scoped, while this system supports nesting, custom scopes, and controlled executor regions
---

## Use Cases
The architecture is suitable for:

* Multi-tenant backend services
* Request correlation identifiers
* Unit-of-work flows
* Event handler pipelines
* Domain service execution contexts
* Worker and background job context management
* Plugin sandboxed environments
* Message-driven systems (RabbitMQ, Kafka, ServiceBus, Wolverine/Marten)
---

## Summary
The architecture provides:

* Deterministic context activation
* Safe async boundary propagation
* Strict type isolation
* Correct cleanup semantics
* Secure read-only access for untrusted code
* DI-composable lifecycle control

This makes the **Context System** robust enough for production-grade, distributed, multi-tenant and plugin-based applications.

---

> **Built with ❤️ for .NET developers**