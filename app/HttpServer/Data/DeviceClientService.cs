using Protocol;

namespace HttpServer.Data
{
    public class DeviceClientService
    {
        DeviceClient deviceClient = null;
 
        public Task<string> SendStringCommand(string target, string command)
        {
            if (deviceClient == null)
            {
                deviceClient = new DeviceClient(new NullLogger());
                deviceClient.Connect("localhost", 21008, null, Guid.NewGuid());
            }
            var responseText = deviceClient.SendStringRequest(target, command);
            return Task.FromResult(responseText);
        }
    }
}
