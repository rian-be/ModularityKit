using System.Collections.Concurrent;
using Core.Features.Context.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polygon.Core.Context;
using Polygon.Signals;
using Signals.Core.Bus;
using Signals.Core.Handlers;
using Signals.Core.Subscriptions;
using Signals.Pipeline;
using Signals.Pipeline.Middleware;
using Signals.Runtime.Interfaces;
using Signals.Runtime.Loader;

// ============================
// DI CONTAINER
// ============================

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.ClearProviders();
  //  builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});


services.AddContext<MyContext>();
services.AddReadOnlyContextAccessor<MyContext>();


// Core storage
services.AddSingleton<ConcurrentDictionary<Type, HandlerCollection>>();
services.AddSingleton<ConcurrentDictionary<Type, RequestHandlerCollection>>();

// Core services
services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
services.AddSingleton<IPublisher, Publisher>();

services.AddSingleton<IEventBus, EventBus>();


// ============================
// MIDDLEWARE PIPELINE
// ============================

services.AddSingleton<IEventMiddleware, LoggingMiddleware>();

services.AddSingleton<IEventMiddleware>(_ =>
    new FilterMiddleware(evt =>
    {
        var prop = evt.GetType().GetProperty("Message");
        var msg = prop?.GetValue(evt) as string;
        return !string.IsNullOrWhiteSpace(msg);
    })
);

// ============================
// PLUGIN SYSTEM
// ============================

services.AddSingleton<IModuleManifestReader, JsonModuleManifestReader>();
services.AddSingleton<IModuleAssemblyLoader, DefaultAssemblyLoader>();
services.AddSingleton<IModuleDependencyResolver, TopologicalDependencyResolver>();
services.AddSingleton<IModuleActivator, DefaultModuleActivator>();

services.AddSingleton<SignalsLoader>();
services.AddSingleton<SignalsDemo>();

// ============================
// BUILD
// ============================

var sp = services.BuildServiceProvider();

var testingContext = TestingContext.Create(sp);
await testingContext.RunDemo();

Console.WriteLine("\n" + new string('═', 70) + "\n");

// Run signals demo
var signalsDemo = sp.GetRequiredService<SignalsDemo>();
await signalsDemo.RunDemo();