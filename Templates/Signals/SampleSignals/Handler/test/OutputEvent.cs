using Signals.Core.Events;

namespace SampleSignals.Handler.test;



    public sealed record OutputEvent(string Message) : IResponseEvent;