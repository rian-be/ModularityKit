# ADR-008: Mutation Interceptor

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Interception

## Context

In the mutation system, there is a need to **observe and react to mutations at different points of their lifecycle**:
- Before mutation execution
- After successful execution
- On failure
- When a mutation is blocked by a policy

This functionality is essential for logging, auditing, monitoring, validation, generating side effects, or integration with external systems.

## Decision

### IMutationInterceptor

**Responsibilities:**

- Represent a single mutation interceptor with a unique name and a defined execution order (`Order`).
- Provide methods invoked at lifecycle points:
    - `OnBeforeMutationAsync` — before mutation execution
    - `OnAfterMutationAsync` — after successful mutation
    - `OnMutationFailedAsync` — on exception
    - `OnPolicyBlockedAsync` — when a mutation is blocked by a policy

**Properties:**

- `Name` — unique interceptor name
- `Order` — execution order (lower values execute earlier)

### Design Rationale

- Separates observation and reaction logic from the core mutation mechanism.
- Allows modular addition of auditing, validation, monitoring.
- Ensures deterministic invocation order and enables layered interceptors.
- Interceptors can be used in both production and test environments.