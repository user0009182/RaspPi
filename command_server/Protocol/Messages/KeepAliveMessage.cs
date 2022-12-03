using System;

namespace Protocol
{
    //keepalive message
    //1 byte: 2
    public class KeepAliveMessage : BaseMessage
    {
        public KeepAliveMessage() : base(DeviceProtocolMessageType.KeepAlive)
        {
        }
    }
}
