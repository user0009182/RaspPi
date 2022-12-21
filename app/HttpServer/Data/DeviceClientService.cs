using Protocol;
using System.Collections.Concurrent;

namespace HttpServer.Data
{
    public class DeviceClientService
    {
        DeviceClient deviceClient = null;
 
        BlockingCollection<ResponseMessage> responseMessages = new BlockingCollection<ResponseMessage>();

        public string SendStringCommand(string target, string command)
        {
            if (deviceClient == null)
            {
                deviceClient = new DeviceClient("web", null);
                deviceClient.Connect("localhost", 21008, null, Guid.NewGuid());
                deviceClient.StartHandler(OnMessageReceived, OnFailure);
            }
            deviceClient.SendStringRequest(target, command);
            var responseMsg = responseMessages.Take();
            return System.Text.Encoding.ASCII.GetString(responseMsg.RequestData);
        }

        void OnMessageReceived(BaseMessage message)
        {
            var responseMsg = message as ResponseMessage;
            if (responseMsg != null)
            {
                responseMessages.Add(responseMsg);
            }
        }

        void OnFailure(DeviceClient client)
        {
            deviceClient = null;
        }
    }
}
