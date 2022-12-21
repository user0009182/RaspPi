using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Terminal
{
    class Terminal
    {
        uint nextRequestId = 0;
        BlockingCollection<string> commands = new BlockingCollection<string>();
        BlockingCollection<string> responses = new BlockingCollection<string>();
        BlockingCollection<ResponseMessage> responseMessages = new BlockingCollection<ResponseMessage>();
        internal void Run(string host, int port)
        {
            Task.Run(() => NetThreadProc(host, port));
            Console.WriteLine($"connected to {host}:{port}");
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                commands.Add(command);
                if (command == "exit")
                    break;
                var response = responses.Take();
                Console.WriteLine(response);
            }
        }

        void NetThreadProc(string host, int port)
        {
            var device = new DeviceClient("Terminal", null);
            device.Connect(host, port, null, Guid.Empty);
            device.StartHandler(OnMessageRecieved, OnFailure);
            while (true)
            {
                string command;
                if (!commands.TryTake(out command, 2000))
                {
                    continue;
                }
                if (command == "exit")
                    break;
                var parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                nextRequestId++;
                if (parts[0] == "send")
                {
                    var targetDeviceName = parts[1];
                    DoSend(device, nextRequestId, targetDeviceName, parts.Skip(2));
                }
                else
                {
                    device.Writer.SendMessage(new RequestMessage(nextRequestId, null, device.RemoteDeviceId, Encoding.ASCII.GetBytes(command)));
                }

                var response = WaitForResponse(device, nextRequestId);
                if (response == null)
                    continue;
                var responseString = Encoding.ASCII.GetString(response.RequestData);
                if (command.Contains("get_image"))
                {
                    System.IO.File.WriteAllBytes(@"imageA.jpg", Convert.FromBase64String(responseString));
                }
                responses.Add(responseString);
            }
        }

        void OnFailure(DeviceClient message)
        {
            Console.Write("Failure");
        }

        void OnMessageRecieved(BaseMessage message)
        {
            if (message is ResponseMessage)
            {
                responseMessages.Add((ResponseMessage)message);
            }
        }

        ResponseMessage WaitForResponse(DeviceClient device, uint requestId)
        {
            while (true)
            {
                var response = responseMessages.Take();
                if (response.RequestId == requestId)
                    return response;
            }
        }

        void DoSend(DeviceClient device, uint requestId, string targetName, IEnumerable<string> commandParts)
        {
            try
            {
                var command = string.Join(' ', commandParts);
                device.Writer.SendMessage(new RequestMessage(requestId, targetName, Guid.Empty, Encoding.ASCII.GetBytes(command)));
            }
            catch (Exception e)
            {
                if (IsSocketTimeoutException(e))
                {
                    Console.WriteLine("timeout");
                }
                else
                {
                    Console.WriteLine("error");
                }
            }
        }

        bool IsSocketTimeoutException(Exception e)
        {
            if (!(e is IOException))
                return false;
            var socketException = e.InnerException as SocketException;
            if (socketException == null)
                return false;
            return socketException.SocketErrorCode == SocketError.TimedOut;
        }
    }
}
