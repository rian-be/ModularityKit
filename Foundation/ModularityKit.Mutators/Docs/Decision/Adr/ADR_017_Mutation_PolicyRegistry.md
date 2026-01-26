# ADR-017: Mutation Policy Registry

## Tag
#adr_017 

## Status
Accepted

## Date
2026-01-26

## Scope
ModularityKit.Mutators.Runtime.Policies

## Context

The mutation engine requires **central registry for mutation policies** (`IMutationPolicy<TState>`), which can be applied for different state types (`TState`).

Policies define validations, authorizations, and business rules that must be enforced **before a mutation is executed**.

Requirements:
- Thread safe access.
- Easy addition/removal of policies.
- Fast read access during mutation execution.
- Support multiple policies per state type.

## Decision

### Registry Implementation

- Implement **`PolicyRegistry : IPolicyRegistry`** with thread safe access using `_lock`.
- Policies are stored in dictionary `_policies` keyed by state type with list of policy objects.

**Methods:**
- `Register<TState>(IMutationPolicy<TState> policy)` – adds a new policy for a state type.
- `Unregister<TState>(string policyName)` – removes a policy by name.
- `GetPolicies<TState>()` – returns all policies for a state type.
- `GetPolicy<TState>(string name)` – returns a policy by name or `null` if not found.

### Thread safety & Determinism

- All read/write operations are executed under `_lock` to ensure thread safety.
- Policies are stored in **list** to guarantee deterministic read order.

### Type Safety & Developer Experience

- Generic `IMutationPolicy<TState>` interfaces ensure type safety.
- Developers can easily add/remove policies for a specific state type without risking type conflicts.

## Design Rationale

- Provides **central, thread safe policy registry** for mutation engine.
- Ensures **deterministic execution order** of policies.
- Integrates seamlessly with `MutationEngine` and the mutation pipeline.
- DX friendly: simple, type safe API for developers.

## Related ADRs #adr_012 #adr_013 #adr_015

- ADR-012: Mutation Execution Interfaces and Context Separation
- ADR-013: Mutation Engine and Executor Runtime Integration
- ADR-015: Mutation Interceptor Pipeline
## Consequences

### Positive

- Central, thread safe mutation policy registry.
- Fast and deterministic access to policies for a given state type.
- Simple integration with `MutationEngine`.

### Negative

- In production, very large sets of policies may require monitoring or caching.
- Thread contention may occur if many concurrent registrations/unregistrations happen (rare in typical scenarios).