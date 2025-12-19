using ModularityKit.Context.Abstractions;
using ModularityKit.Context.ReadOnly;
using ModularityKit.Context.Runtime;
using SimpleInjector;

namespace ModularityKit.Context.SimpleInjector;

/// <summary>
/// Provides extension methods to register context-related services into Simple Injector container.
/// </summary>
public static class ContextServiceCollection
{
    /// <param name="container">The <see cref="Container"/> to which context services will be added.</param>
    extension(Container container)
    {
        /// <summary>
        /// Registers services required for managing a <typeparamref name="TContext"/> in dependency injection.
        /// </summary>
        /// <typeparam name="TContext">The type of context, must implement <see cref="IContext"/>.</typeparam>
        /// <param>The Simple Injector
        ///     <name>container</name>
        ///     <see cref="Container"/> to register services into.</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Registers <see cref="ContextStore{TContext}"/> as a singleton for storing current context.</item>
        /// <item>Registers <see cref="IContextAccessor{TContext}"/> as a singleton to access the current context.</item>
        /// <item>Registers <see cref="IContextManager{TContext}"/> as a singleton to execute code within a context.</item>
        /// </list>
        /// </remarks>
        public void AddContext<TContext>()
            where TContext : class, IContext, new()
        {
            container.Register<ContextStore<TContext>>(Lifestyle.Singleton);
            container.Register<IContextAccessor<TContext>, ContextAccessor<TContext>>(Lifestyle.Singleton);
            container.Register<IContextManager<TContext>, ContextManager<TContext>>(Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers a read-only context accessor for untrusted code.
        /// </summary>
        /// <typeparam name="TContext">The type of context, must implement <see cref="IContext"/>.</typeparam>
        /// <param>The Simple Injector
        ///     <name>container</name>
        ///     <see cref="Container"/> to register services into.</param>
        /// <remarks>
        /// This registers <see cref="IContextAccessor{IReadOnlyContext}"/> which wraps the standard
        /// <see cref="IContextAccessor{TContext}"/> to provide read-only access.
        /// </remarks>
        public void AddReadOnlyContextAccessor<TContext>()
            where TContext : class, IContext, new()
        {
            container.Register<IContextAccessor<IReadOnlyContext>>(() =>
            {
                var innerAccessor = container.GetInstance<IContextAccessor<TContext>>();
                return new ReadOnlyContextAccessor<TContext>(innerAccessor);
            }, Lifestyle.Singleton);
        }
    }
}