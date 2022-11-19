using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerCommandHandler
    {
        DeviceServer server;
        private readonly ILogger logger;

        public ServerCommandHandler(DeviceServer server, ILogger logger)
        {
            this.server = server;
            this.logger = logger;
        }

        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        void ThreadProc()
        {
            while(true)
            {
                var request = server.CommandQueue.Take();
                ProcessRequest(request);
            }
        }

        void ProcessRequest(RequestMessage request)
        {
            var command = Encoding.ASCII.GetString(request.RequestData).ToLower().Trim();
            //var parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (command == "ping")
            {
                SendResponse(request, "pong");
                return;
            }

            SendResponse(request, "unknown command");
        }

        void SendResponse(RequestMessage request, string responseText)
        {
            var responseClient = server.GetConnectedDevice(request.SourceDeviceId);
            if (responseClient == null)
            {
                //TODO
                logger.Log($"could not send response to {request.SourceDeviceId}");
                return;
            }
            var response = new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes(responseText));
            responseClient.SendQueue.Add(response);
        }
    }
}
