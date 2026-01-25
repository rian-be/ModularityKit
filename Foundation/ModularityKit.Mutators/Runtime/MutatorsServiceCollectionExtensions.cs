using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Mutators.Abstractions;
using ModularityKit.Mutators.Abstractions.Audit;
using ModularityKit.Mutators.Abstractions.History;
using ModularityKit.Mutators.Abstractions.Metrics;
using ModularityKit.Mutators.Abstractions.Interception;
using ModularityKit.Mutators.Runtime.Audit;
using ModularityKit.Mutators.Runtime.Metrics;
using ModularityKit.Mutators.Runtime.Policies;
using ModularityKit.Mutators.Runtime.Interception;
using ModularityKit.Mutators.Runtime.Loggers;

namespace ModularityKit.Mutators.Runtime;

/// <summary>
/// Extension methods for registering the Mutators framework services into a <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Registers all core components required for the <see cref="IMutationEngine"/>:
/// <list type="bullet">
/// <item><see cref="IMutationExecutor"/></item>
/// <item><see cref="IPolicyRegistry"/></item>
/// <item><see cref="IInterceptorPipeline"/></item>
/// <item><see cref="IMutationAuditor"/></item>
/// <item><see cref="IMutationHistoryStore"/></item>
/// <item><see cref="IMetricsCollector"/></item>
/// </list>
/// Optionally adds the default <see cref="LoggingInterceptor"/>.
/// </para>
/// <para>
/// Usage:
/// <code>
/// var services = new ServiceCollection();
/// services.AddMutators(options =&gt; 
/// {
///     options.AlwaysValidate = true;
///     options.EnableDetailedMetrics = true;
/// }, addDefaultLoggingInterceptor: true);
/// </code>
/// </para>
/// </remarks>
public static class MutatorsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Mutators framework services into the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional configuration action for <see cref="MutationEngineOptions"/>.</param>
    /// <param name="addDefaultLoggingInterceptor">
    /// If <c>true</c>, automatically registers <see cref="LoggingInterceptor"/> for console logging.
    /// </param>
    public static void AddMutators(
        this IServiceCollection services,
        Action<MutationEngineOptions>? configure = null,
        bool addDefaultLoggingInterceptor = false)
    {
        // Core services
        services.AddSingleton<IMutationExecutor, MutationExecutor>();
        services.AddSingleton<IPolicyRegistry, PolicyRegistry>();
        services.AddSingleton<IInterceptorPipeline, InterceptorPipeline>();
        services.AddSingleton<IMutationAuditor, InMemoryAuditor>();
        services.AddSingleton<IMutationHistoryStore, InMemoryHistoryStore>();
        services.AddSingleton<IMetricsCollector, MetricsCollectorImpl>();

        // Optional default logging interceptor
        if (addDefaultLoggingInterceptor)
            services.AddSingleton<IMutationInterceptor, LoggingInterceptor>();

        // MutationEngine registration
        services.AddSingleton<IMutationEngine>(sp =>
        {
            var executor = sp.GetRequiredService<IMutationExecutor>();
            var policies = sp.GetRequiredService<IPolicyRegistry>();
            var interceptors = sp.GetServices<IMutationInterceptor>()
                                 .OrderBy(i => i.Order)
                                 .ToList();
            var auditor = sp.GetRequiredService<IMutationAuditor>();
            var history = sp.GetRequiredService<IMutationHistoryStore>();
            var metrics = sp.GetRequiredService<IMetricsCollector>();

            var options = new MutationEngineOptions();
            configure?.Invoke(options);

            var engine = new MutationEngine(
                executor,
                policies,
                sp.GetRequiredService<IInterceptorPipeline>(),
                auditor,
                history,
                metrics,
                options);

            // Register interceptors into the engine
            foreach (var interceptor in interceptors)
                engine.RegisterInterceptor(interceptor);

            return engine;
        });
    }
}
