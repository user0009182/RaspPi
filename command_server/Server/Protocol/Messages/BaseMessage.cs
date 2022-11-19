using System;

namespace Server
{
    //preliminary protocol
    //1 byte: message type
    //n bytes: message type specific data
    public abstract class BaseMessage
    {
        public DeviceProtocolMessageType Type { get; set; }

        /// <summary>
        /// Device ID of the device that sent the message
        /// </summary>
        public Guid SourceDeviceId { get; set; }
        public BaseMessage(DeviceProtocolMessageType type)
        {
            Type = type;
        }
    }
}
