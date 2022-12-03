using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    /// <summary>
    /// Implementation of the protocol connection handshake from both the server and client side
    /// </summary>
    public class DeviceHandshake
    {
        EventTracer trace;
        public DeviceHandshake(EventTracer tracer)
        {
            this.trace = tracer;
        }

        public bool DoHandshakeAsServer(DeviceClient client, Guid serverDeviceId)
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
                //write "server"
                client.Writer.WriteData16(Encoding.ASCII.GetBytes("server"));
                client.Capabilities = (DeviceCapabilities)client.Reader.ReadBytes(1)[0];
                client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
                var clientDeviceName = Encoding.ASCII.GetString(client.Reader.ReadData8());
                client.AssignRemoteName(clientDeviceName);
                client.Writer.WriteBytes(new byte[] { (byte)DeviceCapabilities.Router });
                //send 16 byte device ID
                client.Writer.WriteBytes(serverDeviceId.ToByteArray());
                client.Writer.WriteData8(Encoding.ASCII.GetBytes(client.LocalName));
                //idle timeout interval in seconds. 0 = off
                //the hub will send keep alive packets frequently enough
                client.Writer.WriteUInt16(10);
                //whether the device should reply to keepalives
                client.Writer.WriteByte(1);
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
                trace.Failure(TraceEventId.DeviceHandshakeAsServerFailure, e.ToString());
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
                client.Writer.WriteData8(Encoding.ASCII.GetBytes(client.LocalName));
                client.Capabilities = (DeviceCapabilities)client.Reader.ReadBytes(1)[0];
                client.RemoteDeviceId = new Guid(client.Reader.ReadBytes(16));
                var serverName = Encoding.ASCII.GetString(client.Reader.ReadData8());
                trace.Detail(TraceEventId.HandshakeAsClientReceiveRemoteName, serverName);
                client.AssignRemoteName(serverName);
                //idle timeout interval in seconds. 0 = off
                //the hub will send keep alive packets frequently enough
                ushort idleTimeoutInterval = client.Reader.ReadUInt16();
                //whether the device should reply to keepalives
                bool replyToKeepalives = client.Reader.ReadBytes(1)[0] > 0;
                client.SetIdleTimeoutPolicy(idleTimeoutInterval, replyToKeepalives);
                client.Writer.WriteBytes(Encoding.ASCII.GetBytes("ok"));
            }
            catch (Exception)
            {
                trace.Failure(TraceEventId.DeviceHandshakeAsClientFailure);
                return false;
            }
            return true;
        }
    }
}
