# ADR-005: Mutation Audit Abstractions

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Audit

## Context

The mutation engine must be fully auditable, enabling tracking of all operations on application state. Each mutation can be executed in different contexts, by different actors, and is subject to security policies and business rules. To maintain consistency, transparency, and compliance, a clear and unambiguous method for recording and retrieving mutation information is required.

Auditing should operate independently of mutation logic and should not introduce runtime or infrastructure dependencies. The audit model must be readable, immutable, and ready to integrate with various backends (databases, event stores, logging systems), while enabling reconstruction of the complete operation flow without constraining specific storage implementations.

## Decision

### IMutationAuditor

**Responsibilities:**

- Record the execution of mutations.
- Provide audit query capabilities.

### MutationAuditEntry

**Responsibilities:**

- Store a complete snapshot of a mutation (intent, context, changes, policy decisions).
- Ensure immutability and suitability as audit evidence.

### Data Categories in MutationAuditEntry

- **Identity & Correlation:** `ExecutionId`, `StateId`, `StateType`
- **Intent & Context:** `MutationIntent`, `MutationContext`
- **Effect Evidence:** `ChangeSet`, `IsSuccess`, `ErrorMessage`
- **Policy Trace:** `PolicyDecisions`
- **Temporal Metrics:** `Timestamp`, `Duration`
- **Source Attribution:** `SourceIpAddress`, `UserAgent`
- **Extensibility:** `Metadata`

### Design Rationale

- Auditing is treated as a separate layer that does not affect mutation execution logic.
- The model is broad enough to support various interceptors and integrations.
- Immutability and structured categorization make audit entries reliable as evidence and facilitate downstream analytics and compliance processes.