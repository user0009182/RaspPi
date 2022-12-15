namespace Protocol
{
    /// <summary>
    /// Caller can inject one of these into the server and be provided with trace events
    /// </summary>
    public interface ITraceSink
    {
        void OnEvent(TraceEvent e);

        /// <summary>
        /// Returning false will prevent OnEvent from being called for the given type of event
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsEventTypeTraced(TraceEventType type);
    }
}
