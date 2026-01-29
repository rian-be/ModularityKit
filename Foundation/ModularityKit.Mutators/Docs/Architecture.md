# Mutators Architecture

This document provides a comprehensive overview of the internal architecture of **ModularityKit.Mutators**.  
The architecture is designed to provide **deterministic, auditable, and policy controlled state mutation**
for .NET applications, without relying on implicit side effects, unordered execution, or ad hoc validation logic.

---

## Architectural Principles

The architecture follows four foundational principles.

### 1. Strong Isolation Per Mutation

Each mutation is treated as an **isolated, explicit unit of change**:

* Explicit input state
* Explicit intent
* Explicit output (ChangeSet / Result)
* No hidden side effects

Mutations do not directly coordinate with each other and do not rely on global state.

---

### 2. Deterministic Execution Flow

The engine guarantees:

* Predictable mutation ordering
* Explicit execution phases
* No hidden execution branches
* Deterministic outcomes for the same input

This ensures reproducibility, debuggability, and auditability.

---

### 3. Explicit Lifecycle Management

Mutation execution is:

* Explicitly started
* Executed inside a controlled engine scope
* Observed by policies and interceptors
* Finalized deterministically

There is no implicit mutation execution or ambient behavior.

---

### 4. Decision vs Execution Separation

Two clearly separated concerns exist:

* **Decision phase** – whether a mutation may execute
* **Execution phase** – how the mutation modifies state

This prevents mutation logic from being polluted with policy checks,
ordering concerns, or cross-mutation guards.

---

## Component Architecture

### MutationEngine

`MutationEngine` is the orchestration core.

**Responsibilities:**

* Drives the full mutation lifecycle
* Coordinates decision, execution, and auditing
* Ensures deterministic execution order
* Guarantees cleanup and finalization

**It does not:**

* Decide business rules
* Perform state validation inline
* Encode domain policies

---

### IMutationContext

The mutation context represents the **execution scope** of a mutation run.

**Responsibilities:**

* Holds execution metadata (execution id, timestamps)
* Carries shared runtime services
* Provides access to mutation-scoped facilities (metrics, audit, history)

The context is created **once per engine execution** and disposed deterministically.

---

### IMutationDecisionPipeline

This component is responsible for the **decision phase**.

**Responsibilities:**

* Evaluates whether a mutation is allowed to execute
* Aggregates:
  * policies
  * interceptors
  * mutation preconditions
* Produces a deterministic decision result

**Output:**

```text
Allowed | Blocked
+ reasons
```

This pipeline never mutates state.

---
### IMutationExecutor

The executor performs the actual state mutation.

**Responsibilities:**

- Applies mutation logic
- Produces ChangeSet / Result
- Does not perform permission or policy checks

Executors assume that all decisions have already been made.
IMutationPolicy

Policies express global or cross-cutting constraints.

**Examples:**

- Time based execution windows
- Environment restrictions
- Compliance rules
- Safety guards

**Policies:**

- Participate only in the decision phase
- Never mutate state
- Can block execution with a reason

---
### IMutationInterceptor

Interceptors observe execution without owning decisions.

**Responsibilities:**

- Pre execution observation
- Post execution observation
- logging, tracing
- Auditing hooks

They must not alter mutation outcomes.

---
### IMutationAuditor

The auditor records what happened and why.

**Responsibilities:**

- Records decisions
- Records executed mutations
- Persists results and reasons
- Enables post-mortem analysis

Audit data is first class architectural concern.

---
### Execution Macro-Flow

```
Application Code
      │
      ▼
MutationEngine.Execute
      │
      ▼
Decision Phase
(IMutationDecisionPipeline)
      │
      ├─ policies
      ├─ interceptors
      └─ mutation preconditions
      │
      ▼
Decision Result
(Allowed / Blocked)
      │
      ▼
Execution Phase
(IMutationExecutor)
      │
      ▼
ChangeSet / Result
      │
      ▼
Audit & Finalization
```
This flow is linear, explicit, and deterministic.

---
### Dependency Injection Composition

```csharp
services.AddMutators();
```

**Registered components:**

- Singleton MutationEngine
- Scoped IMutationContext
- Singleton IMutationDecisionPipeline
- Scoped IMutationExecutor
- Singleton IMutationAuditor
- Collection of IMutationPolicy
- Collection of IMutationInterceptor

---
### Why Singleton + Scoped is Correct

- The engine is stateless and orchestrational
- Context is execution-scoped
- No mutation data is stored globally
- Parallel executions are fully isolated

**The architecture is safe for:**

- Multi threaded workloads
- Concurrent requests
- Background processing

---
### Why Mutations Do Not Decide for Themselves

**Mutations must not:**

- Inspect global state to block execution
- Know about other mutations
- Enforce policies internally

**Reasons:**

- Hidden coupling
- Undebuggable behavior
- Impossible to audit decisions
- Unclear failure causes

All decisions live **outside mutation logic**.

---
### Use Cases

The Mutators architecture is suitable for:

- Complex domain workflows
- Policy driven state transitions
- Multinstep approval systems
- Infrastructure orchestration
- Domain driven change execution
- Regulated or audited systems
- Event driven and message based processing

---
### Summary

The Mutators architecture provides:

- Deterministic mutation execution
- Clear separation of concerns
- Explicit decision and execution phases
- Strong auditability
- Policy driven safety
- Predictable and testable behavior

---

> **Built with ❤️ for .NET developers**