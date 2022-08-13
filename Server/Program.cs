using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start(21008);

            while (true)
            {
                var command = Console.ReadLine();
                if (command == "list")
                {
                    ListDevices(server);
                    continue;
                }

                var response = server.SendCommand(command);
                if (response == null)
                {
                    Console.WriteLine("no response");
                }
                else
                {
                    Console.WriteLine(response);
                }
            }
        }

        static void ListDevices(Server server)
        {
            var devices = server.GetConnectedDevices();
            Console.WriteLine($"{devices.Count} connected devices");
            if (devices.Count == 0)
                return;
            foreach (var device in server.GetConnectedDevices())
            {
                Console.WriteLine(device);
            }
        }
    }
}