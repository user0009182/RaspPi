using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class DeviceProtocolWriter
    {
        BinaryWriter writer;
        public DeviceProtocolWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void WriteMessage(BaseMessage message)
        {
            switch (message.Type)
            {
                case DeviceProtocolMessageType.RequestSimple:
                    WriteSimpleRequest(message);
                    break;
                case DeviceProtocolMessageType.ResponseSimple:
                    WriteSimpleResponse(message);
                    break;
                default:
                    throw new DeviceProtocolException($"Attempted to write unknown message type {message.Type}");
            }
        }

        void WriteSimpleRequest(BaseMessage message)
        {
            WriteMessageType(DeviceProtocolMessageType.RequestSimple);
            WriteData16(message.Data);
            //Debug.WriteLine("sent " + Encoding.ASCII.GetString(data));
        }

        void WriteSimpleResponse(BaseMessage message)
        {
            WriteMessageType(DeviceProtocolMessageType.ResponseSimple);
            WriteData16(message.Data);
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
    }
}
