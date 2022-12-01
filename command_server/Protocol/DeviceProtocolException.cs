using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class DeviceProtocolException : Exception
    {
        public DeviceProtocolException(string message) : base(message)
        {
        }

        public static bool IsSocketTimeoutException(Exception e)
        {
            if (!(e is IOException))
                return false;
            var socketException = e.InnerException as SocketException;
            if (socketException == null)
                return false;
            return socketException.SocketErrorCode == SocketError.TimedOut;
        }
    }
}
