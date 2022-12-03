namespace Protocol
{
    public interface ITraceSink
    {
        void OnEvent(TraceEvent e);
        bool IsEventTypeTraced(TraceEventType type);
    }
}
