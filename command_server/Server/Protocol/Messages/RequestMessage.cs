using System;

namespace Server
{
    //request message
    //1 byte: 0
    //4 byte: request ID
    //1 byte: length of target name
    //        if this is 255 then the target is a 16 byte GUID
    //n bytes: target name or GUID
    //4 bytes: length of request data
    //n bytes: request data
    public class RequestMessage : BaseMessage
    {
        public uint RequestId { get; }
        public string TargetDeviceName { get; }
        public Guid TargetDeviceId { get; }
        public byte[] RequestData { get; }

        public RequestMessage(uint requestId, string targetName, byte[] targetGuid, byte[] requestData) : base(DeviceProtocolMessageType.Request)
        {
            this.RequestId = requestId;
            this.TargetDeviceName = targetName;
            if (targetGuid != null)
            {
                TargetDeviceId = new Guid(targetGuid);
            }
            else
            {
                TargetDeviceId = Guid.Empty;
            }
            this.RequestData = requestData;
        }
    }
}
