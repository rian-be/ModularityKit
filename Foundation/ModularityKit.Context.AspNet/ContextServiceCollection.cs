using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Context.Abstractions;
using ModularityKit.Context.ReadOnly;
using ModularityKit.Context.Runtime;

namespace ModularityKit.Context.AspNet;

/// <summary>
/// Provides extension methods to register context-related services into the DI container.
/// </summary>
public static class ContextServiceCollection
{
    /// <param name="services">The <see cref="IServiceCollection"/> to which context services will be added.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers services required for managing a <typeparamref name="TContext"/> in dependency injection.
        /// </summary>
        /// <typeparam name="TContext">The type of context, must implement <see cref="IContext"/>.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Registers <see cref="ContextStore{TContext}"/> as a singleton for storing current context.</item>
        /// <item>Registers <see cref="IContextAccessor{TContext}"/> as a singleton to access the current context.</item>
        /// <item>Registers <see cref="IContextManager{TContext}"/> as a singleton to execute code within a context.</item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddContext<TContext>()
            where TContext : class, IContext
        {
            services.AddSingleton<ContextStore<TContext>>();
            services.AddSingleton<IContextAccessor<TContext>, ContextAccessor<TContext>>();
            services.AddSingleton<IContextManager<TContext>, ContextManager<TContext>>();
        
            return services;
        }

        /// <summary>
        /// Registers read-only context accessor for untrusted code.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddReadOnlyContextAccessor<TContext>()
            where TContext : class, IContext
        {
            services.AddSingleton<IContextAccessor<IReadOnlyContext>>(sp =>
            {
                var innerAccessor = sp.GetRequiredService<IContextAccessor<TContext>>();
                return new ReadOnlyContextAccessor<TContext>(innerAccessor);
            });
        
            return services;
        }
    }
}