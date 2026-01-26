# ADR-001: StateChange and ChangeSet Model

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Changes

## Context

Mutators in the system must introduce and track state changes in a clear and auditable way. Each mutation can modify multiple fields or objects, and these changes can be of different types (addition, removal, modification, replacement, move). To maintain consistency and enable auditing or potential reconstruction of operations, a standard model is needed to represent a single change as well as a set of changes within a single mutation.

## Decision

### StateChange

**Responsibilities:**

- Represent single change in the state.
- Store the value before and after the change.
- Specify the type of change (Modified, Added, Removed, Replaced, Moved).
- Optionally store metadata and priority for conflict resolution.
- Ensure immutability of the instance after creation.

### ChangeSet

**Responsibilities:**

- Aggregate all changes introduced by a mutation.    
- Allow queries by path (`GetChanges`, `IsChanged`, `GetChangedPaths`).
- Provide an easy way to create empty, single, or multi-change sets.
- Optionally include `Checksum` field for integrity.

### Design Rationale

- Separating a single change (`StateChange`) from a set of changes (`ChangeSet`) simplifies auditing and mutation analysis.
- The model is flexible enough to support various mutation tracking scenarios.
- Aggregating changes in `ChangeSet` makes it easy to use in auditing systems, state history, CQRS, or unit tests.
- Change types (`ChangeType`) provide a clear indication of the operation, simplifying application logic and integration tools.