namespace Server
{
    //response message
    //1 byte: 1
    //4 byte: request ID
    //4 bytes: length of response data
    //n bytes: response data
    public class ResponseMessage : BaseMessage
    {
        private uint requestId;
        private byte[] requestData;

        public ResponseMessage(uint requestId, byte[] requestData) : base(DeviceProtocolMessageType.Response)
        {
            this.requestId = requestId;
            this.requestData = requestData;
        }
    }
}
