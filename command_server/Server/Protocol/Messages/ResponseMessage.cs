namespace Server
{
    //response message
    //1 byte: 1
    //4 byte: request ID
    //4 bytes: length of response data
    //n bytes: response data
    public class ResponseMessage : BaseMessage
    {
        public uint RequestId { get; }
        public byte[] RequestData { get; }

        public ResponseMessage(uint requestId, byte[] requestData) : base(DeviceProtocolMessageType.Response)
        {
            this.RequestId = requestId;
            this.RequestData = requestData;
        }
    }
}
