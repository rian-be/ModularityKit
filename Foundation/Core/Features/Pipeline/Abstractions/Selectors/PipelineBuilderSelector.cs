using Core.Features.Pipeline.Runtime;
using Core.Features.Pipeline.Runtime.Compilation;

namespace Core.Features.Pipeline.Abstractions.Selectors;

/// <summary>
/// Provides a factory for creating <see cref="PipelineBuilder{TContext}"/> instances
/// with different execution strategies.
/// </summary>
/// <typeparam name="TContext">The type of context used by the pipeline.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Use <see cref="Live"/> to create a pipeline executing middleware dynamically at runtime.</item>
/// <item>Use <see cref="Compiled()"/> to create a pipeline compiled with the default <see cref="DefaultPipelineCompiler{TContext}"/>.</item>
/// <item>Use <see cref="Compiled(IPipelineCompiler{TContext})"/> to create a pipeline compiled with a custom compiler.</item>
/// <item>Compiled pipelines execute faster as middleware is precompiled into a single delegate.</item>
/// </list>
/// </remarks>
public sealed class PipelineBuilderSelector<TContext>
{
    /// <summary>
    /// Creates a new <see cref="PipelineBuilder{TContext}"/> that executes middleware dynamically at runtime.
    /// </summary>
    /// <returns>A live, dynamic <see cref="PipelineBuilder{TContext}"/> instance.</returns>
    public PipelineBuilder<TContext> Live() => new();

    /// <summary>
    /// Creates a new <see cref="PipelineBuilder{TContext}"/> that compiles the middleware chain
    /// using the default <see cref="DefaultPipelineCompiler{TContext}"/>.
    /// </summary>
    /// <returns>A compiled <see cref="PipelineBuilder{TContext}"/> instance.</returns>
    public PipelineBuilder<TContext> Compiled() => new(new DefaultPipelineCompiler<TContext>());

    /// <summary>
    /// Creates a new <see cref="PipelineBuilder{TContext}"/> that compiles the middleware chain
    /// using a custom <see cref="IPipelineCompiler{TContext}"/>.
    /// </summary>
    /// <param name="compiler">The compiler to use for pipeline compilation.</param>
    /// <returns>A compiled <see cref="PipelineBuilder{TContext}"/> instance using the provided compiler.</returns>
    public PipelineBuilder<TContext> Compiled(IPipelineCompiler<TContext> compiler) => new(compiler);
}