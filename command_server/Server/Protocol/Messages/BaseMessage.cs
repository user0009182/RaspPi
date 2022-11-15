namespace Server
{
    //preliminary protocol
    //1 byte: message type
    //n bytes: message type specific data
    public abstract class BaseMessage
    {
        public DeviceProtocolMessageType Type { get; set; }
        public BaseMessage(DeviceProtocolMessageType type)
        {
            Type = type;
        }
    }

}
