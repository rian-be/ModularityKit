# Core Concepts

**ModularityKit.Mutators** provides a structured, deterministic, and policy-controlled
mechanism for executing **explicit state mutations** in .NET applications.
This document explains the core concepts, execution model, and internal responsibilities.

---

## Overview

A **Mutation Execution** represents a controlled unit of work in which one or more
mutations are evaluated, executed, audited, and finalized.

Each execution:

- has a clear start and end
- runs inside an explicit engine scope
- is subject to decision rules and policies
- produces auditable results

Mutators do not rely on implicit behavior, unordered execution, or hidden side effects.
All mutation-related state is scoped to the execution and managed by the engine.

---

## Goals of Mutators

The design is guided by four core objectives:

### 1. **Isolation per mutation**
Each mutation is an independent unit of change with:
- explicit input
- explicit intent
- explicit output

Mutations do not coordinate with each other implicitly.

---

### 2. **Deterministic execution**
For the same input and policies:
- the same mutations execute
- in the same order
- with the same outcome

No hidden branching or implicit execution paths exist.

---

### 3. **Clear separation of concerns**
Mutators strictly separate:
- decision (whether a mutation may run)
- execution (how state is changed)
- observation (audit, metrics, history)

This keeps mutation logic small and domain-focused.

---

### 4. **Explicit lifecycle and cleanup**
Every mutation execution:
- is explicitly started
- runs inside a controlled scope
- is finalized deterministically
- leaves no residual state

---

## **MutationEngine**

`MutationEngine` is the orchestration core of the system.

Key characteristics:

* Stateless orchestrator
* Entry point for all mutation executions
* Coordinates decision, execution, and auditing
* Guarantees deterministic finalization

The engine does **not** encode business rules or mutation logic.

---

## **Mutation Execution Scope**

A mutation execution represents a **single, isolated run** of the engine.

It carries:
- execution identifier
- timestamps
- engine-scoped services
- audit and metrics hooks

All mutations executed within a run share the same execution scope.

---

## **Decision Phase**

Before any mutation executes, the engine enters the **decision phase**.

### Purpose
To determine **whether a mutation is allowed to execute**.

### Inputs
- mutation intent
- registered policies
- mutation preconditions
- execution context

### Output
A deterministic decision:
```text
Allowed | Blocked
+ reasons
```

No state is mutated during this phase.

---
## **Mutation Policies**

Policies express **global or cross cutting constraints**.

Examples:

- business hours restrictions
- environment safety rules
- compliance guards
- rate or quota limits

Characteristics:

- evaluated during the decision phase
- never mutate state
- may block execution with an explicit reason

---
## **Mutation Execution Phase**

Only mutations that pass the decision phase are executed.

Responsibilities:

- apply mutation logic
- produce ChangeSet or Result
- report execution outcome

Mutation code:

- assumes it is allowed to run
- does not recheck policies
- does not inspect other mutations

---

## **Mutation Interceptors**

Interceptors observe mutation execution.

Typical use cases:

- logging
- metrics
- tracing
- diagnostics

Interceptors:

- run before and/or after execution
- do not influence decisions
- do not alter mutation outcomes

---

## **Mutation Audit**

Auditing is a first class concept.

The auditor records:

- which mutations were requested
- which were blocked or allowed
- why decisions were made
- what changes were applied

This enables:

- traceability
- debugging
- compliance
- post mortem analysis

---

## **Mutation Lifecycle Model**

Each mutation follows a strict lifecycle:
```
[REQUESTED] → [DECIDED] → [EXECUTED] → [AUDITED]
```
1. **Requested**  
    Mutation intent is submitted to the engine.
2. **Decided**  
    Policies and preconditions are evaluated.
3. **Executed**  
    Mutation logic is applied (if allowed).
4. **Audited**  
    Outcome and reasons are persisted.

There are no hidden transitions or implicit retries.

---

## Error Handling

If mutation execution throws:

- the exception is propagated
- the execution is still audited
- the engine finalizes deterministically

Blocked mutations:

- do not throw by default
- produce structured decision results
- remain observable and explainable

---

## **Concurrency and Isolation**

Key characteristics:

1. Each engine execution is isolated.
2. Parallel executions do not share mutable state.
3. Mutations do not rely on thread affinity.
4. No execution data leaks across runs.

This makes Mutators safe for:

- concurrent requests
- message handlers
- event driven pipelines

---

## **Summary**

Mutators provide:

- Explicit mutation execution
- Deterministic decision-making
- Strong separation of responsibilities
- Full auditability
- Safe concurrent execution
- Minimal and predictable APIs

They are well suited for:

- domain workflows
- approval systems
- infrastructure orchestration
- regulated environments
- complex state transitions
- message driven architectures

---
> **Built with ❤️ for .NET developers**