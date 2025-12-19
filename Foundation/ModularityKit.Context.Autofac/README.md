# ModularityKit.Context.Autofac

An **Autofac integration** for **ModularityKit.Context**, providing dependency injection support for context management in the Modularity framework.

---

## Overview

`ModularityKit.Context.Autofac` allows you to register and manage context objects in an **Autofac** container. It provides:

- Registration of `ContextStore<TContext>` for storing the current context.
- `IContextAccessor<TContext>` for accessing the current context.
- `IContextManager<TContext>` to execute code within a context.
- Read-only context accessors for untrusted code.

This package is designed to work seamlessly with **Autofac**, enabling structured context propagation across your application.

---

## Installation

```bash
dotnet add package ModularityKit.Context.Autofac
```
Or via NuGet:
```bash
Install-Package ModularityKit.Context.Autofac
```

## License
> MIT License. See [LICENSE](LICENSE) for details.
