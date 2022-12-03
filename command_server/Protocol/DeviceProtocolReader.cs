using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Protocol
{
    /// <summary>
    /// Provides methods to read device protocol messages over a connection with a remote device or hub
    /// </summary>
    public class DeviceProtocolReader
    {
        public const int MAX_SIMPLE_REQUEST_DATA_LENGTH = 18192;

        BinaryReader reader;
        public DeviceProtocolReader(BinaryReader reader)
        {
            this.reader = reader;
        }

        public BaseMessage ReceiveMessage()
        {
            DeviceProtocolMessageType messageType = ReadMessageType();
            switch (messageType)
            {
                case DeviceProtocolMessageType.Request:
                    return ReceiveRequestMessage();
                case DeviceProtocolMessageType.Response:
                    return ReceiveResponseMessage();
                case DeviceProtocolMessageType.KeepAlive:
                    return new KeepAliveMessage();
                default:
                    throw new DeviceProtocolException($"Unrecognised message type {messageType}");
            }
        }

        RequestMessage ReceiveRequestMessage()
        {
            uint requestId = reader.ReadUInt32();
            byte targetNameLength = reader.ReadByte();
            string targetName=null;
            Guid targetGuid = Guid.Empty;
            if (targetNameLength == 255)
            {
                targetGuid = new Guid(reader.ReadBytes(16));
            }
            else if (targetNameLength > 0)
            {
                targetName = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(targetNameLength));
            }

            uint requestDataLength = reader.ReadUInt32();
            //todo protect against attack
            var requestData = reader.ReadBytes((int)requestDataLength);
            var request = new RequestMessage(requestId, targetName, targetGuid, requestData);
            return request;
        }

        ResponseMessage ReceiveResponseMessage()
        {
            uint requestId = reader.ReadUInt32();
            uint requestDataLength = reader.ReadUInt32();
            //todo protect against attack
            var requestData = reader.ReadBytes((int)requestDataLength);
            var response = new ResponseMessage(requestId, requestData);
            return response;
        }

        internal byte[] ReadBytes(int numBytes)
        {
            return reader.ReadBytes(numBytes);
        }

        public byte[] ReadData16()
        {
            ushort dataLength = reader.ReadUInt16();
            if (dataLength > MAX_SIMPLE_REQUEST_DATA_LENGTH)
            {
                Debug.WriteLine($"Received length {dataLength} exceeds MAX_RECV_PACKET_SIZE {MAX_SIMPLE_REQUEST_DATA_LENGTH}");
                throw new DeviceProtocolException($"Message length {dataLength} exceeds MAX_SIMPLE_REQUEST_DATA_LENGTH");
            }

            var data = reader.ReadBytes(dataLength);
            return data;
        }

        public byte[] ReadData8()
        {
            ushort dataLength = reader.ReadByte();
            var data = reader.ReadBytes(dataLength);
            return data;
        }

        internal ushort ReadUInt16()
        {
            return reader.ReadUInt16();
        }

        DeviceProtocolMessageType ReadMessageType()
        {
            ushort type = reader.ReadByte();
            return (DeviceProtocolMessageType)type;
        }

        internal void SetTimeout(int timeoutMs)
        {
            reader.BaseStream.ReadTimeout = timeoutMs;
        }
    }
}
