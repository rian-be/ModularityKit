using Core.Features.Pipeline.Runtime;
using Core.Features.Pipeline.Runtime.Compilation;

namespace Core.Features.Pipeline.Abstractions.Selectors;

/// <summary>
/// Provides a factory to create multiple <see cref="PipelineBuilder{T}"/> instances
/// for two different context types in a single operation.
/// </summary>
/// <typeparam name="T1">The first pipeline context type.</typeparam>
/// <typeparam name="T2">The second pipeline context type.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Use <see cref="Live"/> to create dynamic pipelines for both contexts.</item>
/// <item>Use <see cref="Compiled()"/> to create pipelines compiled with the default <see cref="DefaultPipelineCompiler{T}"/>.</item>
/// <item>Use <see cref="Compiled(IPipelineCompiler{T1}, IPipelineCompiler{T2})"/> to provide custom compilers for each pipeline.</item>
/// </list>
/// </remarks>
public sealed class PipelineBatchSelector<T1, T2>
{
    private static PipelineBuilder<T> CreateLive<T>() => new();
    private static PipelineBuilder<T> CreateCompiled<T>(IPipelineCompiler<T>? compiler = null) =>
        new(compiler ?? new DefaultPipelineCompiler<T>());

    /// <summary>
    /// Creates two live, dynamic pipelines for <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>) Live() =>
        (CreateLive<T1>(), CreateLive<T2>());

    /// <summary>
    /// Creates two compiled pipelines using the default compiler for each context.
    /// </summary>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>) Compiled() =>
        (CreateCompiled<T1>(), CreateCompiled<T2>());

    /// <summary>
    /// Creates two compiled pipelines using the provided custom compilers.
    /// </summary>
    /// <param name="c1">The compiler for the first pipeline (<typeparamref name="T1"/>).</param>
    /// <param name="c2">The compiler for the second pipeline (<typeparamref name="T2"/>).</param>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>) Compiled(IPipelineCompiler<T1> c1, IPipelineCompiler<T2> c2) =>
        (CreateCompiled(c1), CreateCompiled(c2));
}

/// <summary>
/// Provides a factory to create multiple <see cref="PipelineBuilder{T}"/> instances
/// for three different context types in a single operation.
/// </summary>
/// <typeparam name="T1">The first pipeline context type.</typeparam>
/// <typeparam name="T2">The second pipeline context type.</typeparam>
/// <typeparam name="T3">The third pipeline context type.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Use <see cref="Live"/> to create dynamic pipelines for all three contexts.</item>
/// <item>Use <see cref="Compiled()"/> to create pipelines compiled with the default <see cref="DefaultPipelineCompiler{T}"/>.</item>
/// <item>Use <see cref="Compiled(IPipelineCompiler{T1}, IPipelineCompiler{T2}, IPipelineCompiler{T3})"/> to provide custom compilers for each pipeline.</item>
/// </list>
/// </remarks>
public sealed class PipelineBatchSelector<T1, T2, T3>
{
    private static PipelineBuilder<T> CreateLive<T>() => new();
    private static PipelineBuilder<T> CreateCompiled<T>(IPipelineCompiler<T>? compiler = null) =>
        new(compiler ?? new DefaultPipelineCompiler<T>());

    /// <summary>
    /// Creates three live, dynamic pipelines for <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
    /// </summary>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>, PipelineBuilder<T3>) Live() =>
        (CreateLive<T1>(), CreateLive<T2>(), CreateLive<T3>());

    /// <summary>
    /// Creates three compiled pipelines using the default compiler for each context.
    /// </summary>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>, PipelineBuilder<T3>) Compiled() =>
        (CreateCompiled<T1>(), CreateCompiled<T2>(), CreateCompiled<T3>());

    /// <summary>
    /// Creates three compiled pipelines using the provided custom compilers.
    /// </summary>
    /// <param name="c1">The compiler for the first pipeline (<typeparamref name="T1"/>).</param>
    /// <param name="c2">The compiler for the second pipeline (<typeparamref name="T2"/>).</param>
    /// <param name="c3">The compiler for the third pipeline (<typeparamref name="T3"/>).</param>
    public (PipelineBuilder<T1>, PipelineBuilder<T2>, PipelineBuilder<T3>) Compiled(
        IPipelineCompiler<T1> c1, IPipelineCompiler<T2> c2, IPipelineCompiler<T3> c3) =>
        (CreateCompiled(c1), CreateCompiled(c2), CreateCompiled(c3));
}
