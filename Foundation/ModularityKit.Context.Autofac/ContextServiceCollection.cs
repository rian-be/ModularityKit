using Autofac;
using ModularityKit.Context.Abstractions;
using ModularityKit.Context.ReadOnly;
using ModularityKit.Context.Runtime;

namespace ModularityKit.Context.Autofac;

/// <summary>
/// Provides extension methods to register context-related services into an Autofac container.
/// </summary>
public static class ContextServiceCollection
{
    /// <param name="builder">The Autofac container builder to register services into.</param>
    extension(ContainerBuilder builder)
    {
        /// <summary>
        /// Registers services required for managing a <typeparamref name="TContext"/> in Autofac.
        /// </summary>
        /// <typeparam name="TContext">The type of context, must implement <see cref="IContext"/>.</typeparam>
        public void RegisterContext<TContext>()
            where TContext : class, IContext, new()
        {
            // Core services
            builder.RegisterType<ContextStore<TContext>>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ContextAccessor<TContext>>()
                .As<IContextAccessor<TContext>>()
                .SingleInstance();

            builder.RegisterType<ContextManager<TContext>>()
                .As<IContextManager<TContext>>()
                .SingleInstance();
        }

        /// <summary>
        /// Registers read-only context accessor for untrusted code.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        public void RegisterReadOnlyContextAccessor<TContext>()
            where TContext : class, IContext, new()
        {
            builder.Register(c =>
                {
                    var innerAccessor = c.Resolve<IContextAccessor<TContext>>();
                    return new ReadOnlyContextAccessor<TContext>(innerAccessor);
                })
                .As<IContextAccessor<IReadOnlyContext>>()
                .SingleInstance();
        }
    }
}