﻿using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerCommandHandler
    {
        Server server;
        private readonly ILogger logger;

        public ServerCommandHandler(Server server, ILogger logger)
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
            if (command == "list")
            {
                ListDevicesCommand(request);
                return;
            }

            SendResponse(request, "unknown command");
        }

        void ListDevicesCommand(RequestMessage request)
        {
            var devices = server.GetConnectedDevices();
            var sb = new StringBuilder();
            sb.AppendLine($"{devices.Count} connected devices");
            foreach (var device in devices)
            {
                sb.AppendLine($"{device.Name} {device.DeviceId} {device.IpAddress}");
            }
            SendResponse(request, sb.ToString());
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
