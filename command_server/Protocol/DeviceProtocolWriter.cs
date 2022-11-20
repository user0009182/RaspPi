using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class DeviceProtocolWriter
    {
        BinaryWriter writer;
        public DeviceProtocolWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void SendMessage(BaseMessage message)
        {
            switch (message.Type)
            {
                case DeviceProtocolMessageType.Request:
                    SendRequestMessage(message as RequestMessage);
                    break;
                case DeviceProtocolMessageType.Response:
                    SendResponseMessage(message as ResponseMessage);
                    break;
                default:
                    throw new DeviceProtocolException($"Attempted to write unknown message type {message.Type}");
            }
        }

        void SendRequestMessage(RequestMessage message)
        {
            WriteMessageType(DeviceProtocolMessageType.Request);
            writer.Write((uint)message.RequestId);
            if (message.TargetDeviceName != null)
            {
                var data = System.Text.Encoding.ASCII.GetBytes(message.TargetDeviceName);
                writer.Write((byte)data.Length);
                if (data.Length > 0)
                    writer.Write(data);
            }
            else
            {
                writer.Write((byte)255);
                writer.Write(message.TargetDeviceId.ToByteArray());
            }
            writer.Write((uint)message.RequestData.Length);
            writer.Write(message.RequestData);
        }

        void SendResponseMessage(ResponseMessage message)
        {
            WriteMessageType(DeviceProtocolMessageType.Response);
            writer.Write((uint)message.RequestId);
            writer.Write((uint)message.RequestData.Length);
            writer.Write(message.RequestData);
            //Debug.WriteLine("sent " + Encoding.ASCII.GetString(data));
        }

        void WriteMessageType(DeviceProtocolMessageType type)
        {
            writer.Write((byte)type);
        }

        public void WriteData16(byte[] data)
        {
            writer.Write((ushort)data.Length);
            writer.Write(data);
        }

        internal void SetTimeout(int timeoutMs)
        {
            writer.BaseStream.WriteTimeout = timeoutMs;
        }

        internal void WriteBytes(byte[] data)
        {
            writer.Write(data);
        }
    }
}
