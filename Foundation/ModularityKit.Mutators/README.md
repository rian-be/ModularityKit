# ModularityKit.Mutators

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

A deterministic, async safe mutation engine with built in policy enforcement, audit logging, and execution context.

---

## Features

- **Deterministic State Mutations** – Apply and simulate state changes safely
- **Policy Enforcement** – Declarative, composable mutation policies
- **Async Safe Execution** – Works across `async/await` boundaries
- **Immutable State Models** – Encourages safe, concurrent operations
- **Audit & Change Tracking** – `ChangeSet` captures granular modifications
- **High Performance** – Minimal overhead per mutation execution

---

## Quick Start (Example)

```csharp
using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Runtime;
using ModularityKit.Mutators.Runtime.Loggers;
using Mutators.Examples.BillingQuotas.Policies;
using Mutators.Examples.IamRoles.Policies;
using Mutators.Examples.WorkflowApprovals.Policies;
using IamTwoManApprovalPolicy = Mutators.Examples.IamRoles.Policies.RequireTwoManApprovalPolicy;
using FeatureFlagsTwoManApprovalPolicy = Mutators.Examples.FeatureFlags.Policies.RequireTwoManApprovalPolicy;

var services = new ServiceCollection();

// 1. Register Mutators engine with options
services.AddMutators(MutationEngineOptions.Strict, addDefaultLoggingInterceptor: true);

// 2. Build DI provider
var provider = services.BuildServiceProvider();
var engine = provider.GetRequiredService<IMutationEngine>();

// 3. Register policies
engine.RegisterPolicy(new MaxQuotaPolicy());
engine.RegisterPolicy(new PreventNegativeQuotaPolicy());
engine.RegisterPolicy(new IamTwoManApprovalPolicy());
engine.RegisterPolicy(new PreventLastAdminRemovalPolicy());
engine.RegisterPolicy(new FeatureFlagsTwoManApprovalPolicy());
engine.RegisterPolicy(new EnforceOrderPolicy());
engine.RegisterPolicy(new RequireManagerApprovalPolicy());

// 4. Execute example scenarios
await Examples.FeatureFlags.Scenarios.EnableNewCheckoutScenario.Run(engine);
await Examples.BillingQuotas.Scenarios.EmergencyIncreaseScenario.Run(engine);
await Examples.IamRoles.Scenarios.GrantAdminScenario.Run(engine);

// 5. Inspect history
var history = await engine.GetHistoryAsync(stateId: "EnableNewCheckout");
MutationHistoryLogger.LogHistory(history);

// 6. Metrics & statistics
var stats = await engine.GetStatisticsAsync();
Console.WriteLine($"Total executed: {stats.TotalExecuted}");
Console.WriteLine($"Average execution time: {stats.AverageExecutionTime.TotalMilliseconds:F2} ms");
```

___
## Core Concepts

### **Mutation**

Represents single atomic change to specific state.

- Implement `IMutation<TState>`
- Define intent (`MutationIntent`)
- Implement `Validate(TState)`
- Implement `Apply(TState)` and optionally `Simulate(TState)`

___
### **State**

Immutable representation of domain data.

- use `record` types.
- Concurrent safe
- Represents the source of truth for mutations

___
### **Policy**

Controls which mutations are allowed.

- Implement `IMutationPolicy<TState>`
- Evaluate mutations before application
- Return `PolicyDecision.Allow()` or `PolicyDecision.Deny(reason)`

---
## Best Practices

1. **Immutable State** – Always use `record` types or read-only properties.
2. **Explicit Context** – Pass `MutationContext` per mutation.
3. **Validate Before Apply** – Call `Validate()` before applying a mutation.
4. **Enforce Policies** – Never skip policy evaluation.
5. **Scoped Execution** – Execute mutations inside a controlled engine.
6. **Do Not Share Mutable State** – Each logical operation gets its own state snapshot.
7. **Use Clear IDs for Tracking** – Helps with audit logs and debugging.
8. **Centralized Registration** – Register all policies at engine startup.

---
## API Reference

### Core Interfaces

- `IMutation<TState>` – Base interface for mutations
- `IMutationPolicy<TState>` – Policy controlling allowed mutations
- `IMutationEngine` – Engine for applying mutations
- `MutationContext` – Context carrying metadata about execution
- `MutationResult<TState>` – Wraps the new state and change set
- `ChangeSet` – Captures state modifications

---
## Metrics & Logging

The engine supports:
- Mutation execution history (`GetHistoryAsync`)
- Execution statistics (`GetStatisticsAsync`)
- Logging via `MutationHistoryLogger`
- Optional interceptors for audit and diagnostics

---
## Architecture Decision Records (ADR)

Key architectural decisions for **ModularityKit.Mutators** are tracked as ADRs. They document engine design, policy evaluation, context handling, change tracking, and DI registration.

| ADR     | Title                       | Summary                                                                                      |
| ------- | --------------------------- | -------------------------------------------------------------------------------------------- |
| ADR-001 | Mutation Engine Design      | Defines `IMutationEngine`, engine options, strict vs lenient execution modes                 |
| ADR-002 | Policy Evaluation           | Centralized policy evaluation for validation, risk assessment, and allow/deny decisions      |
| ADR-003 | Context & MutationContext   | Explicit, per execution flow context for audit, tracing, and tenant isolation                |
| ADR-004 | ChangeSet Model             | Immutable, granular state changes for audit, rollback, and history inspection                |
| ADR-005 | Mutation Audit Abstractions | Structured, immutable audit entries capturing intent, context, changes, and policy decisions |

See full ADR documentation in [`Docs/Decision/Adr`](Docs/Decision/listadr) for details on each architectural decision.
