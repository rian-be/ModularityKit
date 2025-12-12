using Core.Features.Context.Abstractions;

namespace Core.Features.Context.ReadOnly;

/// <summary>
/// Immutable snapshot of a context exposing only read-only properties.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IReadOnlyContext"/> to provide a safe, non-mutable view of a context.</item>
/// <item>Can be used to pass context to untrusted code or for logging and auditing purposes.</item>
/// <item>Supports creation from any <see cref="IContext"/> instance via <see cref="FromContext"/>.</item>
/// </list>
/// </remarks>
public sealed record ReadOnlyContextSnapshot(
    string Id,
    DateTimeOffset CreatedAt
) : IReadOnlyContext
{
    /// <summary>
    /// Creates a <see cref="ReadOnlyContextSnapshot"/> from a full <see cref="IContext"/> instance.
    /// </summary>
    /// <param name="context">The original context to snapshot.</param>
    /// <returns>A new <see cref="ReadOnlyContextSnapshot"/> containing the <see cref="IReadOnlyContext"/> view.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static ReadOnlyContextSnapshot FromContext(IContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new ReadOnlyContextSnapshot(
            Id: context.Id,
            CreatedAt: context.CreatedAt
        );
    }
}