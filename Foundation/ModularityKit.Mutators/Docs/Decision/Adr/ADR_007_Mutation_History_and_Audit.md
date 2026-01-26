# ADR-007: Mutation History and Audit

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.History

## Context

In the mutation system, it is necessary to audit all state-changing operations to:
- Ensure a complete change history for accountability and compliance.
- Enable reconstruction of state at any point in time.
- Provide statistics and reports for monitoring and analysis.

Mutations executed via `MutationIntent` and `IMutation<TState>` modify objects in the system. The history of these mutations must be maintained in an organized manner to allow later querying and aggregation.

## Decision

### MutationHistoryEntry

**Responsibilities:**

- Represent a single mutation in the state history.
- Store `ExecutionId`, `MutationIntent`, `MutationContext`, `ChangeSet`, timestamp, and execution duration.
- Optionally store hashes of previous and new state for integrity verification.

### MutationHistory

**Responsibilities:**

- Aggregate `MutationHistoryEntry` records for a given state.
- Allow state reconstruction (`Replay`, `ReplayUntil`).
- Provide a timeline of changes for specific paths (`GetTimelineForPath`).
- Generate statistics (`GetStatistics`).

### IMutationHistoryStore

**Responsibilities:**

- Provide persistent storage of mutation history.
- Enable retrieval of full history, history within a specific time range, and the last N mutations.

### HistoryStatistics

- Aggregate data such as total number of mutations, number of unique actors, distribution of mutations by category, and average number of changes per mutation.

### Design Rationale

- Separating mutation history from execution logic enables independent auditing, analysis, and monitoring.
- Allows reconstruction of any object state over time, which is critical for accountability and testing.
- Facilitates generation of statistics and timelines for reporting and observability purposes.