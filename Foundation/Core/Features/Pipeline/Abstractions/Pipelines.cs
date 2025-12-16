using Core.Features.Pipeline.Abstractions.Selectors;

namespace Core.Features.Pipeline.Abstractions;

/// <summary>
/// Provides entry points to create pipeline builder selectors for specific context types.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Use <see cref="For{TContext}"/> to create a <see cref="PipelineBuilderSelector{TContext}"/> for a single context.</item>
/// <item>Use <see cref="Batch{T1, T2}"/> or <see cref="Batch{T1, T2, T3}"/> to create multiple pipelines for multiple contexts at once.</item>
/// <item>Encapsulates creation of selectors to simplify pipeline setup and ensure type safety.</item>
/// </list>
/// </remarks>
public static class Pipelines
{
    /// <summary>
    /// Creates a new <see cref="PipelineBuilderSelector{TContext}"/> for the specified context type.
    /// </summary>
    /// <typeparam name="TContext">The type of context the pipeline will operate on.</typeparam>
    /// <returns>A <see cref="PipelineBuilderSelector{TContext}"/> instance to build pipelines for <typeparamref name="TContext"/>.</returns>
    public static PipelineBuilderSelector<TContext> For<TContext>() => new();

    /// <summary>
    /// Creates a new <see cref="PipelineBatchSelector{T1, T2}"/> for two context types.
    /// </summary>
    /// <typeparam name="T1">The first pipeline context type.</typeparam>
    /// <typeparam name="T2">The second pipeline context type.</typeparam>
    /// <returns>A <see cref="PipelineBatchSelector{T1, T2}"/> instance to build pipelines for both contexts.</returns>
    public static PipelineBatchSelector<T1, T2> Batch<T1, T2>() => new();

    /// <summary>
    /// Creates a new <see cref="PipelineBatchSelector{T1, T2, T3}"/> for three context types.
    /// </summary>
    /// <typeparam name="T1">The first pipeline context type.</typeparam>
    /// <typeparam name="T2">The second pipeline context type.</typeparam>
    /// <typeparam name="T3">The third pipeline context type.</typeparam>
    /// <returns>A <see cref="PipelineBatchSelector{T1, T2, T3}"/> instance to build pipelines for all three contexts.</returns>
    public static PipelineBatchSelector<T1, T2, T3> Batch<T1, T2, T3>() => new();
}
