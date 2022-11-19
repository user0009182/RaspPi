using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MessageRouter
    {
        DeviceServer server;
        public MessageRouter(DeviceServer server)
        {
            this.server = server;
        }

        public Guid GetDeviceIdByName(string name)
        {
            return Guid.Empty;
        }
    }
}
