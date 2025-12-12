using Signals.Core.Events;

namespace SampleSignals.Handler.test;


public sealed record TestEvent(string Message) : IEvent;