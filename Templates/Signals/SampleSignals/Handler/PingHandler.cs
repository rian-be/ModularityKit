using SampleSignals.Handler.test;
using SampleSignals.Model.Ping;
using Signals.Attributes;
using Signals.Core.Context;
using Signals.Core.Events;

namespace SampleSignals.Handler;

[HandlesRequest(typeof(PingEvent), typeof(PongEvent))]
public class PingHandler : IRequestHandler<PingEvent, PongEvent>
{
    public async Task<PongEvent> Handle(PingEvent evt, SignalContext ctx)
    { 
        var resp2 = await ctx.EmitSingle<TestEvent, OutputEvent>(
            new TestEvent("Hello!")
        ); 
        
        Console.WriteLine($"Got : {resp2.Message}");

        var resp = await ctx.EmitSingle<PingEvent, PongEvent>(
            new PingEvent("Hello plugin!")
        );
        Console.WriteLine($"Got respsdsdsdsonse: {resp.Message}");
       
      // await ctx.EmitSingleRaw(new ExampleEvent($"ExampleEvent s received: {evt.Message}"));
     //  var response = await ctx.EmitResponse<PongEvent>(new PingEvent("Hello plugin!"));
      // Console.WriteLine($"Got single response: {response.Message}");
       
       return new PongEvent($"Pong to: {evt.Message}");
    }
}