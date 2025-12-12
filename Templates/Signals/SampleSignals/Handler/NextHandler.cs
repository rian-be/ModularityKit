using SampleSignals.Handler.test;
using Signals.Attributes;
using Signals.Core.Context;
using Signals.Core.Events;

namespace SampleSignals.Handler;


[HandlesRequest(typeof(TestEvent), typeof(OutputEvent))]
public class NextHandler : IRequestHandler<TestEvent, OutputEvent>
{
    public Task<OutputEvent> Handle(TestEvent request, SignalContext ctx)
    {
        return Task.FromResult(new OutputEvent("Pong"));
    }
}