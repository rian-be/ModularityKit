using Core.Features.Context.Abstractions;
using Core.Features.Context.ReadOnly;

namespace Core.Features.Context.Extensions;

/// <summary>
/// Provides helper extensions for executing logic inside a managed context
/// using read-only projections to enforce immutability.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Offers safe execution wrappers for trusted and untrusted code paths.</item>
/// <item>Prevents mutation of the underlying context through <see cref="IReadOnlyContext"/>.</item>
/// <item>Integrates with <see cref="IContextManager{TContext}"/> to guarantee scope isolation.</item>
/// </list>
/// </remarks>
public static class ContextExtensions
{
    /// <summary>
    /// Creates a read-only view of the provided context.
    /// </summary>
    /// <param name="context">Source context to wrap.</param>
    /// <returns>Read-only wrapper of the context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static IReadOnlyContext AsReadOnly(this IContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ReadOnlyContextView.FromContext(context);
    }

    /// <param name="manager">Context manager that controls scope.</param>
    /// <typeparam name="TContext">Concrete context type.</typeparam>
    extension<TContext>(IContextManager<TContext> manager) where TContext : class, IContext
    {
        /// <summary>
        /// Executes the given action inside a context scope while exposing
        /// a read-only projection of that context to the provided delegate.
        /// </summary>
        /// <param name="context">Context to activate for the duration of the action.</param>
        /// <param name="action">Callback receiving a read-only context view.</param>
        public async Task ExecuteWithReadOnly(TContext context,
            Func<IReadOnlyContext, Task> action)
        {
            await manager.ExecuteInContext(context, async () =>
            {
                var readOnlyView = context.AsReadOnly();
                await action(readOnlyView);
            });
        }

        /// <summary>
        /// Executes untrusted or sandboxed logic inside a controlled context
        /// while exposing only a read-only view of the active context.
        /// </summary>
        /// <typeparam name="TResult">Return type of the untrusted function.</typeparam>
        /// <param name="context">Context to activate during execution.</param>
        /// <param name="untrustedFunc">Untrusted callback receiving a read-only context.</param>
        /// <returns>Result returned by the untrusted function.</returns>
        public async Task<TResult> ExecuteSandboxed<TResult>(TContext context,
            Func<IReadOnlyContext, Task<TResult>> untrustedFunc)
        {
            return await manager.ExecuteInContext(context, async () =>
            {
                var readOnlyView = context.AsReadOnly();
                return await untrustedFunc(readOnlyView);
            });
        }
    }
}
