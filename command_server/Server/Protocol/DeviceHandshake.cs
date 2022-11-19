using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class DeviceHandshake
    {
        ILogger logger;
        public DeviceHandshake(ILogger logger)
        {
            this.logger = logger;
        }

        public bool DoHandshakeAsServer(DeviceClient client, Guid serverDeviceId)
        {
            logger.Log($"begin device protocol handshake as server");
            //TODO reader.settimeout
            var data = client.Reader.ReadData16();
            if (data == null)
                return false;
            if (Encoding.ASCII.GetString(data) != "device")
                return false;
            //TODO timeout catch IOException on receive?
            client.Writer.WriteData16(Encoding.ASCII.GetBytes("server"));
            client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
            //send 16 byte device ID
            client.Writer.WriteBytes(serverDeviceId.ToByteArray());
            data = client.Reader.ReadBytes(2);
            if (data == null)
                return false;
            if (Encoding.ASCII.GetString(data) != "ok")
                return false;
            logger.Log($"device protocol handshake complete OK");
            return true;
        }

        public bool DoHandshakeAsClient(DeviceClient client, Guid serverDeviceId)
        {
            logger.Log($"begin device protocol handshake as client");
            client.Writer.WriteData16(Encoding.ASCII.GetBytes("device"));
            var data = client.Reader.ReadData16();
            if (Encoding.ASCII.GetString(data) != "server")
                return false;
            //send 16 byte device ID
            client.Writer.WriteBytes(serverDeviceId.ToByteArray());
            client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
            logger.Log($"connected to device {client.RemoteDeviceId}");
            client.Writer.WriteBytes(Encoding.ASCII.GetBytes("ok"));
            logger.Log($"handshake complete OK");
            return true;
        }
    }
}
