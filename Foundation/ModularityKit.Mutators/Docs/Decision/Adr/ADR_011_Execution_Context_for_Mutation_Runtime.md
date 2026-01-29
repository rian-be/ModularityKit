# ADR-011: Execution Context for Mutation Runtime

## Tag
#adr_011 

## Status
Accepted

## Date / Last edit
2026-01-22 / 2026-01-26

## Scope
ModularityKit.Mutators.Abstractions.Context  

## Context

ADR-002 introduced **MutationContext** as a _semantic and audit context_ for mutations, answering questions such as:
- Who initiated the mutation?
- Why was the mutation executed?
- In which mode (simulate / validate / commit)?
- With which correlation identifiers?

During the design of the mutation engine, there emerged the need for a **second, orthogonal context**, responsible exclusively for **runtime execution aspects**:
- Identifying the specific execution instance
- Measuring execution time
- Handling timeouts
- Cooperative cancellation
- Storing temporary runtime data for the duration of the execution pipeline

Combining these responsibilities with `MutationContext` would lead to:
- Overgrowth of the context model
- Mixing audit concerns with runtime concerns
- Difficulties in testing, simulation, and batch execution
- Ambiguities when executing the same mutation multiple times

## Decision

### ExecutionContext

We introduced a separate type, `ExecutionContext`, representing a **single execution of a mutation**.

**Responsibilities:**

- Store unique execution identifier (`ExecutionId`)
- Record the start timestamp (`StartedAt`)
- Handle timeout (`Timeout`) and check for expiration (`IsTimedOut()`)
- Support cooperative cancellation (`CancellationToken`)
- Provide runtime data dictionary (`Data`) accessible by:
    - The engine
    - Executors
    - Interceptors
    - Runtime infrastructure

### Relation to MutationContext (ADR-002)

- `MutationContext` describes the **intent and origin of the mutation**
- `ExecutionContext` describes the **specific execution of that mutation in time**
- These two contexts are:
    - **Logically connected**
    - **Technically separated**
    - Passed together through `IMutationEngine` and `IMutationExecutor`

A single `MutationContext` instance **can be reused**:
- In simulation
- In validation
- In actual commit

Each time paired with **different `ExecutionContext`**.

### Design Principles

- `ExecutionContext` **does not contain** information about actors, reasons, or policies
- `MutationContext` **does not contain** runtime information (timeouts, cancellation, execution id)
- `ExecutionContext` is created by the runtime engine, not by the API user
- `ExecutionContext` is treated as **ephemeral runtime state**, not audit data

## Consequences

### Positive

- Clear separation between mutation semantics and execution mechanics
- Enables multiple, deterministic executions of the same mutation
- Improved testability (mocking time, timeouts, cancellations)
- Consistent with pipeline and batch execution architecture
- Natural place for technical runtime data (metrics, trace IDs, runtime cache)

### Negative

- Requires passing two contexts in runtime API
- Slightly increases the number of context types in the system