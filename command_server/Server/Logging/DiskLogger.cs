using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub
{
    public class DiskLogger : ITraceSink
    {
        StreamWriter streamWriter;
        public DiskLogger(string filepath)
        {
            streamWriter = new StreamWriter(File.OpenWrite(filepath));
        }

        public bool IsEventTypeTraced(TraceEventType type)
        {
            return true;
        }

        public void OnEvent(TraceEvent e)
        {
            streamWriter.WriteLine(e.Id);
        }
    }
}
