using Core.Features.Context.Abstractions;

namespace Core.Features.Context.ReadOnly;

/// <summary>
/// Provides a read-only accessor wrapper that exposes <see cref="IReadOnlyContext"/> 
/// while delegating the underlying context retrieval to an existing accessor.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Wraps an <see cref="IContextAccessor{TContext}"/> and converts retrieved contexts into read-only views.</item>
/// <item>Ensures consumers cannot mutate the returned context instance.</item>
/// <item>Ensures thread-safety and consistent snapshot creation.</item>
/// </list>
/// </remarks>
public sealed class ReadOnlyContextAccessor<TContext>(IContextAccessor<TContext> innerAccessor)
    : IContextAccessor<IReadOnlyContext>
    where TContext : class, IContext
{
    /// <inheritdoc />
    public IReadOnlyContext? Current
    {
        get
        {
            var context = innerAccessor.Current;
            return context != null 
                ? ReadOnlyContextSnapshot.FromContext(context)
                : null;
        }
    }

    /// <inheritdoc />
    public IReadOnlyContext RequireCurrent()
    {
        var context = innerAccessor.RequireCurrent();
        return ReadOnlyContextSnapshot.FromContext(context);
    }
}