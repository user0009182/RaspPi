using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TestTraceSink : ITraceSink
    {
        public BlockingCollection<TraceEvent> Events { get; } = new BlockingCollection<TraceEvent>();
        public bool IsEventTypeTraced(TraceEventType type)
        {
            return true;
        }

        public void OnEvent(TraceEvent e)
        {
            Events.Add(e);
        }

        public bool Contains(TraceEventId eventId)
        {
            return Events.Any(e => e.Id == eventId);
        }
    }
}
