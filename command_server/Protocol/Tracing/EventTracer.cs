using System.Collections.Generic;

namespace Protocol
{
    public class EventTracer
    {
        Dictionary<TraceEventType, bool> typeEnabled = new Dictionary<TraceEventType, bool>();

        ITraceSink sink;

        public ITraceSink Sink
        {
            get
            {
                return sink;
            }
        }

        public EventTracer(ITraceSink sink)
        {
            this.sink = sink;
        }

        public void Debug(TraceEventId eventId, params string[] args)
        {
            Trace(TraceEventType.Debug, eventId, args);
        }

        public void Flow(TraceEventId eventId, params string[] args)
        {
            Trace(TraceEventType.Flow, eventId, args);
        }

        public void Detail(TraceEventId eventId, params string[] args)
        {
            Trace(TraceEventType.Detail, eventId, args);
        }

        public void Failure(TraceEventId eventId, params string[] args)
        {
            Trace(TraceEventType.Failure, eventId, args);
        }

        public void Error(TraceEventId eventId, params string[] args)
        {
            Trace(TraceEventType.Error, eventId, args);
        }

        public void Trace(TraceEventType type, TraceEventId eventId, params string[] args)
        {
            if (!IsEventTypeTraced(type))
                return;
            sink.OnEvent(new TraceEvent(type, eventId, args));
        }

        bool IsEventTypeTraced(TraceEventType type)
        {
            bool value;
            if (typeEnabled.TryGetValue(type, out value))
                return value;
            value = false;
            if (sink != null)
                value = sink.IsEventTypeTraced(type);
            typeEnabled[type] = value;
            return value;
        }
    }
}
