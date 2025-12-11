using Core.Features.Context.Abstractions;

namespace Core.Features.Context.ReadOnly;

/// <summary>
/// Provides read-only wrapper over a mutable <see cref="IContext"/> instance.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IReadOnlyContext"/> by delegating all members to an inner <see cref="IContext"/>.</item>
/// <item>Prevents mutation by exposing only getters, ensuring safe consumption by external components.</item>
/// <item>Useful when passing context across layers where write-access is undesired.</item>
/// </list>
/// </remarks>
public sealed class ReadOnlyContextView(IContext inner) : IReadOnlyContext
{
    /// <inheritdoc />
    public string Id => inner.Id;
    
    /// <inheritdoc />
    public DateTimeOffset CreatedAt => inner.CreatedAt;

    /// <summary>
    /// Creates read-only view for the provided context.
    /// </summary>
    /// <param name="context">Underlying context instance.</param>
    /// <returns>A read-only wrapper implementing <see cref="IReadOnlyContext"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is null.
    /// </exception>
    public static IReadOnlyContext FromContext(IContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new ReadOnlyContextView(context);
    }
}