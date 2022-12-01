using Protocol;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "localhost";
            int port = 21008;
            if (args.Length == 2)
            {
                host = args[1];
                port = Convert.ToInt32(args[2]);
            }
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

        static BlockingCollection<string> commands = new BlockingCollection<string>();
        static BlockingCollection<string> responses = new BlockingCollection<string>();
        static void NetThreadProc(string host, int port)
        {
            var device = new DeviceClient("Terminal", new Logger());
            device.Connect(host, port, null, Guid.Empty);
            while(true)
            {
                string command;
                if (!commands.TryTake(out command, 2000))
                {
                    var message = device.TryReadMessage(0);
                    if (message != null && message.Type == DeviceProtocolMessageType.KeepAlive)
                    {
                        device.Writer.SendMessage(new KeepAliveMessage());
                    }
                    continue;
                }
                if (command == "exit")
                    break;
                var parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts[0] == "send")
                {
                    var targetDeviceName = parts[1];
                    DoSend(device, targetDeviceName, parts.Skip(2));
                    continue;
                }
                device.Writer.SendMessage(new RequestMessage(1, null, device.RemoteDeviceId, Encoding.ASCII.GetBytes(command)));
                var response = ReadResponse(device);
                if (response == null)
                    continue;
                var responseString = Encoding.ASCII.GetString(response.RequestData);
                responses.Add(responseString);
            }
        }

        static ResponseMessage ReadResponse(DeviceClient device)
        {
            //skip over any messages that aren't responses
            while (true)
            {
                var message = device.TryReadMessage(5000);
                if (message == null)
                    return null;
                var responseMessage = message as ResponseMessage;
                if (responseMessage != null)
                    return responseMessage;
            }
        }

        static uint requestId=0;
        static void DoSend(DeviceClient device, string targetName, IEnumerable<string> commandParts)
        {
            try
            {
                var command = string.Join(' ', commandParts);
                requestId++;
                device.Writer.SendMessage(new RequestMessage(requestId, targetName, Guid.Empty, Encoding.ASCII.GetBytes(command)));
                var response = ReadResponse(device);
                var responseString = Encoding.ASCII.GetString(response.RequestData);
                Console.WriteLine(responseString);
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

        static bool IsSocketTimeoutException(Exception e)
        {
            if (!(e is IOException))
                return false;
            var socketException = e.InnerException as SocketException;
            if (socketException == null)
                return false;
            return socketException.SocketErrorCode == SocketError.TimedOut;
        }
    }

    public class Logger : ILogger
    {
        public void Log(string text)
        {
        }
    }
}
