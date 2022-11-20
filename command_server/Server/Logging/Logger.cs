using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Logger : ILogger
    {
        BlockingCollection<string> log = new BlockingCollection<string>();
        public BlockingCollection<string> Data
        {
            get
            {
                return log;
            }
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
            log.Add(text);
        }
    }
}
