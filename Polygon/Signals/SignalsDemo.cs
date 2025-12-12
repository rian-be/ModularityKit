using SampleSignals.Model.Ping;
using Signals.Core.Bus;
using Signals.Extensions;
using Signals.Runtime.Loader;

namespace Polygon.Signals;

/// <summary>
/// Provides a runnable demonstration of the Signals framework,
/// including plugin loading, dynamic event subscriptions,
/// and the request/response messaging pattern.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Loads external signal modules using <see cref="SignalsLoader"/>.</item>
/// <item>Publishes events dynamically via <see cref="IEventBus"/> without compile-time coupling.</item>
/// <item>Shows request/response workflow using strongly-typed event pairs.</item>
/// <item>Acts as a reference for host applications integrating the Signals runtime.</item>
/// </list>
/// </remarks>
public class SignalsDemo(IEventBus bus, SignalsLoader loader)
{
    /// <summary>
    /// Executes the full demo flow:
    /// plugin discovery, dynamic subscriptions, and request/response.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing asynchronous execution.
    /// </returns>
    public async Task RunDemo()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    Signals Demo                              ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Load plugins
        Console.WriteLine("=== Loading Plugins ===");
        Console.WriteLine("Loading plugins from 'Signals' directory...");
        loader.LoadFromDirectory("Signals");
        Console.WriteLine("Plugins loaded ✓\n");

        // Dynamic subscription example
        await DemoDynamicSubscription();

        Console.WriteLine();

        // Request/Response example
        await DemoRequestResponse();

        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Signals Demo Complete                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
    }

    /// <summary>
    /// Demonstrates runtime-driven event subscriptions and dynamic handlers
    /// resolved through plugin metadata and the <see cref="SignalsLoader"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Creates multiple handlers for the same event.</item>
    /// <item>Publishes event instances using runtime type lookup.</item>
    /// <item>Validates multi-handler dispatch execution path.</item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A <see cref="Task"/> representing asynchronous execution.
    /// </returns>
    private async Task DemoDynamicSubscription()
    {
        Console.WriteLine("=== Dynamic Event Subscription ===");
        
        bus.Subscribe(loader, "SampleSignals", "ExampleEvent", async evt =>
        {
            Console.WriteLine($"Dynamic handler fired: {evt.GetType().Name}");
            await Task.CompletedTask;
        });

        bus.Subscribe(loader, "SampleSignals", "ExampleEvent", async evt =>
        {
            Console.WriteLine($"Dynamic handler fired [#2]: {evt.GetType().Name}");
            await Task.CompletedTask;
        });

        Console.WriteLine("\nPublishing ExampleEvent...");
        await bus.Publish(
            loader,
            "SampleSignals",
            "ExampleEvent",
            "Hello from host!"
        );
        
        Console.WriteLine("ExampleEvent published ✓");
    }

    /// <summary>
    /// Demonstrates request/response communication using typed events.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Executes a <see cref="PingEvent"/> request.</item>
    /// <item>Awaits a corresponding <see cref="PongEvent"/> response from plugin code.</item>
    /// <item>Validates the service contract for synchronous two-way event handling.</item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A <see cref="Task"/> representing asynchronous execution.
    /// </returns>
    private async Task DemoRequestResponse()
    {
        Console.WriteLine("=== Request/Response Pattern ===");
        
        var ping = new PingEvent("Hello plugin!");
        Console.WriteLine($"Sending PingEvent: {ping.Message}");
        
        var pong = await bus.Send<PingEvent, PongEvent>(ping);
        Console.WriteLine($"Received PongEvent: {pong.Message}");
        Console.WriteLine("Request/Response completed ✓");
    }
}
