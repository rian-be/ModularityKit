# ADR-009: Mutation Metrics

## Tag
#adr_009
## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Metrics

## Context

In the mutation system, it is essential to monitor performance, validation efficiency, and the impact of mutations on system state and resources.  
Metrics enable:
- Analysis of mutation execution time and its distribution (percentiles, min/max/average)
- Monitoring the number of state changes processed and rules/policies evaluated
- Observing memory usage and state size
- Analyzing throughput and cache efficiency
- Collecting additional custom metrics for further analysis or integration with monitoring systems    

This allows auditing, optimization, and alerting for performance issues or policy violations.

## Decision

### IMetricsCollector

**Responsibilities:**

- Begin scope for single mutation (`BeginScope`)
- Record mutation metrics (`RecordAsync`)
- Provide collected metrics in aggregated form (`GetAggregatedAsync`)

### IMetricsScope

- Scope associated with a specific `executionId` of a mutation
- Records validation time (`RecordValidationTime`)
- Records policy evaluation time (`RecordPolicyEvaluationTime`)
- Records state size (`RecordStateSize`) and memory usage (`RecordMemoryUsage`)
- Allows adding custom metrics (`AddCustomMetric`)
- Produces final `MutationMetrics` object (`Build`)

### AggregatedMetrics / MutationMetrics

- `AggregatedMetrics` provides statistics over a period: min/max, averages, percentiles, throughput
- `MutationMetrics` contains detailed metrics for a single mutation, including execution time, state changes, memory, cache usage, and additional metrics

### Design Rationale

- Separates metrics collection and reporting from mutation logic
- Enables performance analysis, bottleneck detection, and real time policy monitoring
- Aggregation supports reporting and alerting without impacting the core mutation engine
- Scoping allows precise tracking of metrics for individual mutations within system