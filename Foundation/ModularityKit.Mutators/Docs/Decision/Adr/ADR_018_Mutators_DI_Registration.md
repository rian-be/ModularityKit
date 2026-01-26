# ADR-018: Mutators DI Registration

## Tag
#adr_018 

## Status
Accepted

## Date
2026-01-26

## Scope
ModularityKit.Mutators.Runtime

## Context

The Mutators framework consists of multiple interdependent components:
- `IMutationExecutor`
- `IPolicyRegistry`
- `IInterceptorPipeline`
- `IMutationAuditor`
- `IMutationHistoryStore`
- `IMetricsCollector`

To simplify usage and integration, **central DI registration** mechanism for these services is required (`IServiceCollection`).

Considerations:
- Certain components require **registration order** (eg., interceptors in the pipeline).
- Mutation engine options (`MutationEngineOptions`) should be configurable.
- Optional **default logging interceptor** (`LoggingInterceptor`) should be easily added.

## Decision

- Introduce an **extension method**: `IServiceCollection.AddMutators()`.
- Core Mutators components are registered as **singletons**:

| Interface             | Implementation       |
| --------------------- | -------------------- |
| IMutationExecutor     | MutationExecutor     |
| IPolicyRegistry       | PolicyRegistry       |
| IInterceptorPipeline  | InterceptorPipeline  |
| IMutationAuditor      | InMemoryAuditor      |
| IMutationHistoryStore | InMemoryHistoryStore |
| IMetricsCollector     | MetricsCollectorImpl |
|                       |                      |

- Optionally, `LoggingInterceptor` can be added.
- `IMutationEngine` is registered as singleton and, on initialization:
    - Retrieves all dependencies from DI.
    - Applies `MutationEngineOptions` configuration delegate.
    - Registers interceptors into the engine according to their `Order`.

## Design Rationale

- Centralizes **DI setup** for all Mutators components.
- Ensures **deterministic order** for interceptors in the pipeline.
- Provides **DX friendly default setup** for developers, including optional logging.
- Supports replacing in memory auditors/history stores with production implementations.

## Related ADRs #adr_012 #adr_013 #adr_015

- ADR-012: Mutation Execution Interfaces and Context Separation
- ADR-013: Mutation Engine and Executor Runtime Integration
- ADR-015: Mutation Interceptor Pipeline

## Consequences

### Positive

- All Mutators components are easily available via DI in .NET applications.
- Consistent and deterministic interceptor execution order.
- Centralized configuration of the mutation engine via optional delegate.
- Default logging can be enabled without extra code.

### Negative

- In memory implementations (`InMemoryAuditor`, `InMemoryHistoryStore`) are suitable **only for development and testing**.
- Production scenarios require replacing these implementations during DI registration.