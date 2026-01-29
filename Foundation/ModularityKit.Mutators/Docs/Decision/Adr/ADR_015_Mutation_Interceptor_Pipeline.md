# ADR-015: Mutation Interceptor Pipeline

## Tag
#adr_015

## Status
Accepted

## Date
2026-01-26

## Scope
ModularityKit.Mutators.Runtime.Interception  
ModularityKit.Mutators.Abstractions.Interception

## Context

In mutation system, there is need to add **cross cutting logic** (audit, metrics, logging, additional validation) without modifying core `MutationEngine` or mutations themselves (`IMutation<TState>`).

Requirements include:
- A **deterministic execution order** for multiple interceptors.
- Interceptors reacting to events: `BeforeMutation`, `AfterMutation`, `MutationFailed`, `PolicyBlocked`.
- Not every interceptor must run for every mutation — selective execution via filtering (`ShouldRun`) is needed.

## Decision

We introduce **`IInterceptorPipeline`** as central component for managing interceptors.

**InterceptorPipeline** implements `IInterceptorPipeline` and provides:
- Registration and unregistration of interceptors (`Register`, `Unregister`).
- Deterministic execution order (`Order`).
- Asynchronous invocation of interceptor methods for each event (`OnBeforeMutationAsync`, `OnAfterMutationAsync`, `OnMutationFailedAsync`, `OnPolicyBlockedAsync`).
- Filtering of interceptors via `ShouldRun`.

**MutationInterceptorBase** provides default no op implementation of interceptors, allowing easy creation of custom interceptors:
- Default `Order = 0` and `Name = TypeName`.
- All methods are no-op by default.
- `ShouldRun` can be overridden for selective execution.

## Design Rationale

- Separates cross cutting concerns from core mutations and engine.
- Deterministic order and filtering minimize the risk of unwanted side effects.
- Asynchronous pipeline allows integration with metrics and auditing at runtime.
- Serves as foundation for all future interceptors, including integrations with `InMemoryAuditor` and `InMemoryHistoryStore`.

## Consequences

### Positive
- Easy addition of interceptors without changing the engine.
- Testable and deterministic execution.
- Provides natural place for audit, logging, and metrics.

### Negative

- Performance depends on the number of interceptors and their implementations.
- Requires careful consideration of `Order` and filtering to avoid unintended side effects.