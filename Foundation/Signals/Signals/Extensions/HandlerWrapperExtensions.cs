using System.Collections.Immutable;
using Signals.Core.Bus;
using Signals.Core.Handlers;

namespace Signals.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ImmutableArray{HandlerWrapper}"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Includes helpers for sorting, filtering, and manipulating collections of <see cref="HandlerWrapper"/> instances.</item>
/// <item>Used internally by <see cref="IEventBus"/> implementations to manage handler execution order and selection.</item>
/// <item>Designed for immutable arrays to ensure thread-safe, functional-style operations.</item>
/// </list>
/// </remarks>
internal static class HandlerWrapperExtensions
{
    /// <summary>
    /// Sorts an <see cref="ImmutableArray{HandlerWrapper}"/> by <see cref="HandlerWrapper.Priority"/> descending.
    /// </summary>
    /// <param name="array">Array of handler wrappers to sort.</param>
    /// <returns>New immutable array sorted by priority.</returns>
    public static ImmutableArray<HandlerWrapper> SortByPriority(this ImmutableArray<HandlerWrapper> array)
    {
        return array.IsDefaultOrEmpty
            ? ImmutableArray<HandlerWrapper>.Empty
            : [..array.OrderByDescending(w => w.Priority)];
    }
}