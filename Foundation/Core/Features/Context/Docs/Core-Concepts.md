# Core Concepts

The Context System provides structured, type-safe and async-flow-aware contextual data for your application. This document explains the internal architecture, design goals, and execution model.

---

## Overview

A Context represents the execution environment for a unit of work. It carries metadata such as:
- correlation identifiers
- tenant and user information
- request metadata
- dependency flow state

Contexts are not global and do not use AsyncLocal<T> directly. Instead, each context type is managed independently through a dedicated store and accessor.

## Goals of the Context System

The design is guided by four core objectives:

1. **Isolation per context type**
Each TContext has independent lifecycle and storage.

2. **Async-safe execution**
Context flows across await boundaries without leaking across unrelated operations.
3. **Strong typing**
No casting, no dynamic bags, no string keys.
4. **Clear ownership and deterministic cleanup**
Every context must be explicitly opened and explicitly closed.

---

## **ContextStore<TContext>**

`ContextStore<TContext>` is the low-level container that stores the currently active context.

Key characteristics:

* Per-type instance: each TContext has its own store
* Singleton in DI (intentionally)
* Uses an internal mechanism to scope state per logical async flow
* Does not create or manage contexts — only holds them

Typical operations:
* GetCurrent()
* SetCurrent(TContext?)
* Clear()

Your code should never use this directly; instead, use **IContextAccessor** and **IContextManager**.

---

## **IContextAccessor<TContext>**

**IContextAccessor<TContext>** provides safe access to the active context.

| Method             | Behavior                                 |
|--------------------|------------------------------------------|
| `TryGetCurrent()`  | Returns the context or `null`.           |
| `GetCurrent()`     | Returns context or `null`; no exception. |
| `RequireCurrent()` | Throws if no context is active.          |
| `HasContext()`     | Boolean check.                           |

This ensures that service code can reliably depend on context availability without coupling to its lifecycle management.

## **IContextManager<TContext>**

The manager controls the context lifecycle and ensures proper cleanup:
```csharp
await manager.ExecuteInContext(context, async () =>
{
// context is active here
});
```

Responsibilities:
1. Set the context before execution
2. Ensure context flows correctly across async calls
3. Restore previous context
4. Prevent context leaks on exceptions

A single context is active only within the scope of the delegate passed to `ExecuteInContext`.

---

## **ReadOnlyContextAccessor<TContext>**

Some contexts must be exposed to untrusted or restricted components (e.g., plugins, sandboxed logic, external modules).

`ReadOnlyContextAccessor<TContext>`:
* Wraps a typed accessor
* Converts full context into an IReadOnlyContext
* Removes any access to application-specific fields
* Ensures immutability and isolation

Registration:
```csharp
services.AddReadOnlyContextAccessor<MyContext>();
```
This enables code to read basic context metadata without risk of modification or leakage.

---

##  **Context Lifetime Model**

A context has a strict lifecycle:
```csharp
[CREATE] → [ACTIVATE] → [IN USE] → [CLEANUP]
```

1. **Create**
Application code constructs the context instance.
2. **Activate**
`IContextManager<TContext>.ExecuteInContext()` sets this context as current.
3. **In Use**
All services resolved within the async flow may access the context.
4. **Cleanup**
Manager restores previous context (if stacked) or clears state.

Contexts **cannot** be nested unless your manager implementation explicitly supports stack semantics.
The provided implementation allows proper restoration of previous contexts.
---

## Error Handling

If code inside the context block throws:
* The exception is rethrown
* The context is still cleaned up deterministically
* There is no state corruption or leakage into other requests

This makes it structurally safe for:
* background jobs
* message handlers
* request pipelines
* domain workflows

## **Threading and Async Behavior**

Key characteristics:
1. Context binds to the logical async flow, not physical thread.
2. Context persists across await.
3. Parallel tasks cannot see each other's context.
4. Cleanup ensures no bleeding across execution boundaries.

This design is more predictable than raw `AsyncLocal<T>` while avoiding its common pitfalls.

---

## **Summary**

The Context System provides:
* Type-safe context models
* Deterministic lifecycle
* Async-safe propagation
* Clear read/write separation
* A minimal, reliable API

It is well suited for:
* request correlation
* multi-tenant services
* event/command handlers
* hosted background workers
* plugin-based architectures
---

> **Built with ❤️ for .NET developers**