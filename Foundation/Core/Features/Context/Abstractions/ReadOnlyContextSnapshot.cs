namespace Core.Features.Context.Abstractions;

/// <summary>
/// Immutable snapshot of a context exposing only read-only properties.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IReadOnlyContext"/> to provide a safe, immutable view.</item>
/// <item>Captures <see cref="IContext.Id"/> and <see cref="IContext.CreatedAt"/> at the moment of snapshot creation.</item>
/// <item>Can be used for sandboxed or logging scenarios where context mutation must be prevented.</item>
/// </list>
/// </remarks>
public sealed record ReadOnlyContextSnapshot(
    string Id,
    DateTimeOffset CreatedAt
) : IReadOnlyContext;