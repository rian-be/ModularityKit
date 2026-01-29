# ADR-016: Mutation Metrics Collection

## Tag
#adr_016 

## Status
Accepted

## Date
2026-01-26

## Scope
ModularityKit.Mutators.Runtime.Metrics

## Context

Every mutation in the system should allow measurement of key metrics:
- **ExecutionTime** – total execution time.
- **ValidationTime** – time spent in validation.
- **PolicyEvaluationTime** – time spent evaluating policies.
- **StateSize** – size of the mutated state.
- **MemoryUsed** – memory consumption.
- **AdditionalMetrics** – custom metrics defined per mutation.

Requirements:
- Thread safety.
- Integration with the mutation pipeline.
- Ability to aggregate metrics over time.
- DX friendly interface for developers.

In testing or development environments, metrics can be stored in memory; persistence for production is not required in this version.

## Decision

### MetricsCollector Implementation

- Implemented **`MetricsCollectorImpl : IMetricsCollector`**, storing mutation metrics in memory using a `Dictionary<string, MutationMetrics>` with thread safe locking.
- **`BeginScope(string executionId)`** returns `MetricsScope` for single mutation.
- **`RecordAsync`** saves metrics for given `executionId`.
- **`GetAggregatedAsync(from, to)`** aggregates metrics over a time interval, computing: total mutations, average execution time, min/max, and percentiles (P50, P95, P99).

### MetricsScope

- Allows measuring metrics within the scope of a single mutation.
- Tracks: total execution time, validation time, policy evaluation time, state size, memory usage, and custom metrics.
- **`Build()`** finalizes the scope and returns a `MutationMetrics` object.
- Implements **`IDisposable`** for optional cleanup and future extensibility.

## Design Rationale

- Provides lightweight, in memory, thread safe telemetry system for mutations.
- Scope based design integrates naturally with the mutation pipeline.
- DX friendly API allows developers to measure, collect, and aggregate metrics without infrastructure overhead.

## Related ADRs #adr_012 #adr_013 #adr_015

- ADR-012: Mutation Execution Interfaces and Context Separation
- ADR-013: Mutation Engine and Executor Runtime Integration
- ADR-015: Mutation Interceptor Pipeline

## Consequences

### Positive

- Full thread safe telemetry available in-memory for mutation testing and development.
- Developers can easily create scopes for individual mutations and aggregate historical data.
- No external infrastructure (databases) required in dev/test environments.

### Negative

- Current implementation is in-memory only; production usage requires persistent storage.
- Production version may require retention policies and possibly offloading metrics to an external store.