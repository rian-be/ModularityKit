# ADR-003: MutationIntent and BlastRadius

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Intent

## Context

Mutations in the system can vary in scope and impact on components, modules, or the entire system. To enable auditing, risk control, impact assessment, and decisions regarding validation and approval, a model is needed to describe the **intent** of a mutation. This model should capture the operation name, category, risk, reversibility, estimated impact (blast radius), tags, and additional metadata. Without such a model, it is difficult to enforce consistent security policies, compliance, and review processes.

## Decision

### MutationIntent

**Responsibilities:**

- Represent the purpose and context of a mutation (`OperationName`, `Category`, `Description`).
- Specify risk level and reversibility (`RiskLevel`, `IsReversible`).
- Estimate system impact via `BlastRadius`.
- Support classification and tagging for audit and analytics (`Tags`, `Metadata`).
- Store the creation timestamp of the intent (`CreatedAt`).
- Provide factories to create minimal instances with required fields (`Create`).

### BlastRadius and BlastRadiusScope

**Definition:**

- Represents the potential scope of a mutation.
- Impact scope: `Single` (single element), `Module` (module), `System` (entire system), `Global` (multiple systems).
- Optionally includes the number of affected elements and a descriptive note.

### MutationRiskLevel

**Definition:**

- Defines the risk level of a mutation: `Low`, `Medium`, `High`, `Critical`.
- Helps guide review, approval, and audit decisions.

### Design Rationale

- Separating mutation intent from execution logic allows independent analysis of risk and impact.    
- The model supports auditing, reporting, validation, and review processes in distributed systems.
- Estimated blast radius provides a preliminary assessment of potential impact and reduces the risk of unintended changes.