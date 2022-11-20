using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class DiskLogger : ILogger
    {
        StreamWriter streamWriter;
        public DiskLogger(string filepath)
        {
            streamWriter = new StreamWriter(File.OpenWrite(filepath));
        }

        public void Log(string text)
        {
            streamWriter.WriteLine(text);
        }
    }
}
