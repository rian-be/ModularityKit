# ModularityKit.Context.AspNet

An **ASP.NET Core integration** for **ModularityKit.Context**, providing dependency injection support for context management in ASP.NET applications.

---

## Overview

`ModularityKit.Context.AspNet` allows you to register and manage context objects in the **ASP.NET Core DI container**. It provides:

- Registration of `ContextStore<TContext>` for storing the current context.
- `IContextAccessor<TContext>` for accessing the current context.
- `IContextManager<TContext>` to execute code within a context.
- Read-only context accessors for untrusted code.

This package is designed to work seamlessly with **ASP.NET Core**, enabling structured context propagation across your application.

---

## Installation

```bash
dotnet add package ModularityKit.Context.AspNet
```
Or via NuGet:
```bash
Install-Package ModularityKit.Context.AspNet
```

## License
> MIT License. See [LICENSE](LICENSE) for details.