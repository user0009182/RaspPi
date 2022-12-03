using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MessageRouter
    {
        Server server;
        public MessageRouter(Server server)
        {
            this.server = server;
        }

        public string ForwardServerName = null;

        public Guid ResolveDeviceId(string deviceName)
        {
            Guid targetId = Guid.Empty;
            if (ForwardServerName != null)
            {
                var home = server.GetConnectedDevices().FirstOrDefault(d => d.Name.ToLower() == ForwardServerName);
                if (home != null)
                    return home.DeviceId;
                return targetId;
            }
            var targetDevice = server.GetConnectedDevices().FirstOrDefault(f => f.Name.Equals(deviceName, StringComparison.CurrentCultureIgnoreCase));
            if (targetDevice != null)
            {
                targetId = targetDevice.DeviceId;
                return targetId;
            }
            else
            {
                server.Trace.Failure(TraceEventId.ResolveFailure, deviceName);
                return Guid.Empty;
            }
        }
    }
}
