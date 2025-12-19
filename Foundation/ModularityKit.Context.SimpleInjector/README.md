# ModularityKit.Context.SimpleInjector

A Simple Injector integration for **ModularityKit.Context**, providing dependency injection support for context management in the Modularity framework.

---

## Overview

`ModularityKit.Context.SimpleInjector` allows you to register and manage context objects in a **Simple Injector** container. It provides:

- Registration of `ContextStore<TContext>` for storing the current context.
- `IContextAccessor<TContext>` for accessing the current context.
- `IContextManager<TContext>` to execute code within a context.
- Read-only context accessors for untrusted code.

---

## Installation

```bash
dotnet add package ModularityKit.Context.SimpleInjector
```
Or via NuGet:
```bash
Install-Package ModularityKit.Context.SimpleInjector
```

## License
> MIT License. See [LICENSE](LICENSE) for details.