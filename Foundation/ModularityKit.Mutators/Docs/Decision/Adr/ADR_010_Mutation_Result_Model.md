# ADR-010: Mutation Result Model

## Tag
#adr_010 
## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Results

## Context

The mutation system requires a **unified way of reporting the results of operations on application state**. Each mutation can end in success, validation failure, or be blocked by a policy. Additionally, it is necessary to track:
- Applied changes (`ChangeSet`)
- Validation outcomes (`ValidationResult`)
- Policy decisions (`PolicyDecision`)
- Execution metrics (`MutationMetrics`)

Batch executions of mutations should also aggregate this information, including the number of successes, failures, total time, and merged changes.

## Decision

### MutationResult

- Represents the result of a single mutation.
- Contains information about:    
    - Success (`IsSuccess`)
    - New state (`NewState`)
    - Change set (`Changes`)
    - Validation outcomes (`ValidationResult`)
    - Policy decisions (`PolicyDecisions`)
    - Execution metrics (`Metrics`)
    - Optional exception (`Exception`)
    - Completion time (`CompletedAt`)
- Provides factories `Success()`, `Failure()`, and `PolicyBlocked()` for convenient instance creation.

### ValidationResult and Related Types

- `ValidationResult` aggregates:
    - Errors (`ValidationError`)
    - Warnings (`ValidationWarning`)
    - Info messages (`ValidationInfo`)
- Allows creating results with single or multiple errors (`WithError()`, `WithErrors()`).
- Each validation message includes a path (`Path`), description (`Message`), optional code (`Code`), and severity (`ValidationSeverity`).

### BatchMutationResult

- Aggregates results of multiple mutations in a batch operation.
- Contains:
    - Final state after applying all mutations (`FinalState`)
    - List of individual mutation results (`Results`)
    - Aggregated change set (`AggregatedChanges`)
    - Counts of successes and failures (`SuccessCount`, `FailureCount`)
    - Total execution time (`TotalExecutionTime`)
- Facilitates batch analysis, auditing, and global metric reporting.

### Design Rationale

- Centralizing mutation results enables consistent reporting and easy integration with monitoring and auditing systems.
- Separating validation, changes, and metrics ensures modularity and future extensibility.
- Batch results improve performance and allow analysis of mutations in the context of operation sequences.