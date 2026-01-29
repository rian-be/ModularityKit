# ADR-004: Mutation Policies and PolicyDecision

## Tag
#adr_004 

## Status
Accepted

## Date
2026-01-22

## Scope
ModularityKit.Mutators.Abstractions.Policies

## Context

In the mutation system, a unified mechanism is needed to control whether mutations can be executed. Policies act as a **governance layer**, allowing mutations to be approved, blocked, or modified based on system state, business rules, risk, or compliance requirements. Additionally, policy decisions should include metadata, severity, requirements (e.g., additional approvals), and possible mutation modifications.

## Decision

### IMutationPolicy

**Responsibilities:**

- Evaluate mutations against the current state (`Evaluate`).
- Store the policy name, description, and priority.
- Provide evaluation results in the form of a `PolicyDecision`.

### PolicyDecision

**Responsibilities:**

- Indicate whether the mutation is allowed (`IsAllowed`).
- Provide a reason for the decision (`Reason`).
- Include the policy name (`PolicyName`).
- Specify the severity of the decision (`Severity`).
- Include optional mutation modifications (`Modifications`).
- List requirements that must be fulfilled (`Requirements`).

### PolicyDecisionSeverity

- `Info`, `Warning`, `Error`, `Critical` — define the gravity of the decision and support auditing and alerting.

### PolicyRequirement

- Represents prerequisites that must be met before executing a mutation (e.g., approvals, 2FA).
- Includes type, description, structured data, and fulfillment flag (`IsFulfilled`).
- Factory methods simplify creating common requirements (`Approval`, `TwoFactor`).

### Design Rationale

- Policies provide a unified, auditable governance mechanism.
- Separating policy evaluation from mutation execution enables testing, simulation, and auditing.
- Severity levels and requirements allow dynamic control of mutation workflows and reduce the risk of errors or compliance violations.