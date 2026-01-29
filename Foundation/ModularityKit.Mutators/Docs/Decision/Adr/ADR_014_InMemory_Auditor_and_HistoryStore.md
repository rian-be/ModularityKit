# ADR-014: In-Memory Auditor and History Store

## Tag
#adr_014
## Status
Accepted

## Date
2026-01-26

## Scope
ModularityKit.Mutators.Runtime.Audit  
ModularityKit.Mutators.Abstractions.Audit  
ModularityKit.Mutators.Abstractions.History

## Context

For testing and development purposes, simple, fully in-memory implementations of the auditor and mutation history store are needed.
- **Auditor**: stores `MutationAuditEntry` objects in memory.
- **History**: stores `MutationHistoryEntry` objects in memory keyed by `stateId` or `CorrelationId`.

Implementations must be **thread-safe** and provide fast access for tests. These are **not intended for production use**.

## Decision

### InMemoryAuditor

- Implements `IMutationAuditor`.
- Stores all entries in an in memory list.
- Thread safety ensured via locks.
- Provides methods:
    - `AuditAsync`
    - `GetAuditLogAsync`
    - `GetAllEntries`
    - `Clear`

### InMemoryHistoryStore

- Implements `IMutationHistoryStore`.
- Stores history in a dictionary: `stateId -> List<MutationHistoryEntry>`.
- Thread safety ensured via locks.
- Provides methods:
    - `StoreAsync`
    - `GetHistoryAsync`
    - `GetHistoryRangeAsync`
    - `GetRecentAsync`
    - `Clear`

### Design Rationale

- Minimal, deterministic implementation for testing and development.
- Integrates seamlessly with `MutationEngine` and `MutationExecutor`.
- Supports contexts:
    - `MutationContext` (ADR-002) — for auditing and mutation identification
    - `ExecutionContext` (ADR-011) — for execution times and ephemeral runtime state

## Related ADRs #adr_002 #adr_011 #adr_013

- ADR-002: Mutation Context and Actor Type
- ADR-011: Execution Context for Mutation Runtime
- ADR-013: Mutation Engine and Executor Runtime Integration

## Consequences

### Positive

- Fast and simple testing of the mutation pipeline.
- No dependency on external databases.
- Thread safe via locks.

### Negative

- Not suitable for production use.
- No data durability between application restarts.