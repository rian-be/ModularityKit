# ADR-002: Mutation Context and Actor Type

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Context

## Context

Every mutation in the system requires precise tracking of **who** initiated it, **how**, and **why**. This information is essential for auditing, reporting, root-cause analysis, and correlation in distributed systems, as well as for simulation and validation of mutations. Without a consistent context model, operation traceability can be lost and the mutation history may become ambiguous.

The system needs model that can distinguish different actor types (users, system, services, policies, schedulers), capture the reason for the mutation, timestamp, correlation identifiers, and additional metadata, while remaining easily extensible and usable for auditing and tooling.

## Decision

### MutationContext

**Responsibilities:**

- Store information about the actor performing the mutation (`ActorId`, `ActorName`, `ActorType`).
- Store mutation reason (`Reason`), execution mode (`Mode`), timestamp (`Timestamp`), correlation identifiers (`CorrelationId`, `CausationId`).
- Store additional contextual data (`SessionId`, `SourceIpAddress`, `UserAgent`, `TimeZone`, `Culture`, `Metadata`).
- Provide factory methods for common scenarios (System, User, Service).

### ActorType

**Definition:**

- Enumeration of actors performing a mutation: `Unknown`, `User`, `System`, `Service`, `Policy`, `Scheduler`, `Administrator`.
- Provides unambiguous classification of the mutation source.

### MutationMode

**Definition:**

- Enumeration of mutation execution modes: `Simulate`, `Validate`, `Commit`.
- Distinguishes actual state changes from test runs, validation, or simulations.

### Design Rationale

- Separating mutation context from mutator logic preserves consistency and auditability.
- A standard context model simplifies tracking and reporting changes in a distributed system.
- Context factories (System, User, Service) make it easy to create correct instances and eliminate repetitive code.