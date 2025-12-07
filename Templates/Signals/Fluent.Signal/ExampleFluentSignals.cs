using Signals.ApiBase;
using Signals.ApiBase.Fluent;
using Signals.Attributes;

namespace Fluent.Signal;

[SignalModuleMetadata(
    name: "Example Fluent Signals",
    author: "Sean",
    version: "1.0.0",
    description: "An example module in the fluent style"
)]
public class ExampleFluentSignals : FluentSignalModuleBase
{
    protected override void OnRegister()
    {
        On<FluentExampleEvent>(async evt =>
        {
            Console.WriteLine($"[Fluent] Received: {evt.Message} (Id: {evt.Id})");
            await Task.CompletedTask;
        });
    }
}
