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

        public bool UseForwarding;

        public Guid ResolveDeviceId(string deviceName)
        {
            Guid targetId = Guid.Empty;
            if (UseForwarding)
            {
                var home = server.GetConnectedDevices().FirstOrDefault(d => d.Name == "home");
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
                server.Logger.Log($"target {deviceName} not found");
                return Guid.Empty;
            }
        }
    }
}
