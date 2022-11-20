using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    class DeviceProtocolException : Exception
    {
        public DeviceProtocolException(string message) : base(message)
        {
        }
    }
}
