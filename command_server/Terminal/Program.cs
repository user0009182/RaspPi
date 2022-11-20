using Protocol;
using System;
using System.Text;

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
            var device = new DeviceClient(new Logger());
            device.Connect(host, port, null, Guid.Empty);
            Console.WriteLine($"connected to {host}:{port}");
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                if (command == "exit")
                    break;
                device.Writer.SendMessage(new RequestMessage(1, null, device.RemoteDeviceId, Encoding.ASCII.GetBytes(command)));
                var response = device.Reader.ReceiveMessage() as ResponseMessage;
                var responseString = Encoding.ASCII.GetString(response.RequestData);
                Console.WriteLine(responseString);
            }
        }
    }

    public class Logger : ILogger
    {
        public void Log(string text)
        {
        }
    }
}
