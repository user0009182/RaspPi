using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class DeviceHandshake
    {
        ILogger logger;
        public DeviceHandshake(ILogger logger)
        {
            this.logger = logger;
        }

        public bool DoHandshakeAsServer(DeviceClient client, Guid serverDeviceId, string servername)
        {
            try
            {
                //TODO reader.settimeout
                var data = client.Reader.ReadData16();
                if (data == null)
                    return false;
                if (Encoding.ASCII.GetString(data) != "device")
                    return false;
                //TODO timeout catch IOException on receive?
                client.Writer.WriteData16(Encoding.ASCII.GetBytes("server"));
                client.Capabilities = (DeviceCapabilities)client.Reader.ReadBytes(1)[0];
                client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
                var clientDeviceName = Encoding.ASCII.GetString(client.Reader.ReadData8());
                client.AssignName(clientDeviceName);
                client.Writer.WriteBytes(new byte[] { (byte)DeviceCapabilities.Router });
                //send 16 byte device ID
                client.Writer.WriteBytes(serverDeviceId.ToByteArray());
                client.Writer.WriteData8(Encoding.ASCII.GetBytes(servername));
                data = client.Reader.ReadBytes(2);
                if (data == null)
                    return false;
                if (Encoding.ASCII.GetString(data) != "ok")
                    return false;
            }
            catch (Exception e)
            {
                //TODO
                //IOException with inner exception SocketException, SocketErrorCode::TimedOut indicates timeout
                logger.Log($"device protocol handshake failure");
                return false;
            }
            return true;
        }

        public bool DoHandshakeAsClient(DeviceClient client, Guid serverDeviceId)
        {
            try
            {
                client.Writer.WriteData16(Encoding.ASCII.GetBytes("device"));
                var data = client.Reader.ReadData16();
                if (Encoding.ASCII.GetString(data) != "server")
                    return false;
                client.Writer.WriteBytes(new byte[] { (byte)DeviceCapabilities.Router });
                //send 16 byte device ID
                client.Writer.WriteBytes(serverDeviceId.ToByteArray());
                client.Writer.WriteData8(Encoding.ASCII.GetBytes(client.Name));
                client.Capabilities = (DeviceCapabilities)client.Reader.ReadBytes(1)[0];
                client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
                var serverName = Encoding.ASCII.GetString(client.Reader.ReadData8());
                logger.Log($"connected to device {serverName} {client.RemoteDeviceId}");
                client.Writer.WriteBytes(Encoding.ASCII.GetBytes("ok"));
            }
            catch (Exception)
            {
                logger.Log($"device protocol handshake failure");
                return false;
            }
            return true;
        }
    }
}
