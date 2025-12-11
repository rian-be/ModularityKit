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
            return context != null ? CreateSnapshot(context) : null;
        }
    }

    /// <inheritdoc />
    public IReadOnlyContext RequireCurrent()
    {
        var context = innerAccessor.RequireCurrent();
        return CreateSnapshot(context);
    }
    
    /// <summary>
    /// Creates a read-only snapshot from a mutable context.
    /// </summary>
    /// <param name="context">The mutable context to snapshot.</param>
    /// <returns>A <see cref="ReadOnlyContextSnapshot"/> representing the current state.</returns>
    private static IReadOnlyContext CreateSnapshot(IContext context)
    {
        return new ReadOnlyContextSnapshot(
            Id: context.Id,
            CreatedAt: context.CreatedAt
        );
    }
}