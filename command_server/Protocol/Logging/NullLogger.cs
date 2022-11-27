using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class NullLogger : ILogger
    {
        public void Log(string text)
        {
        }
    }
}
