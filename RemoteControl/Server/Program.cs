using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static string boundIp;
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start(21008);

            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine().ToLower();
                ProcessCommand(server, command);
            }
        }

        static bool ProcessCommand(Server server, string command)
        {
            if (command == "list")
            {
                ListDevices(server);
                return true;
            }
            if (command.StartsWith("bind "))
            {
                var parts = command.Split(' ');
                var ip = parts[1];
                var device = server.GetDevice(ip);
                if (device != null)
                {
                    boundIp = ip;
                    Console.WriteLine("Bound to " + boundIp);
                }
                else
                {
                    Console.WriteLine("Device " + ip + " not found");
                }
                return true;
            }
            if (boundIp == null)
            {
                Console.WriteLine("Not bound to a device");
                return true;
            }
            var response = server.SendCommand(boundIp, command);
            if (response == null)
            {
                Console.WriteLine("no response");
                return true;
            }
            else
            {
                if (command == "?")
                {
                    var supportedCommands=response.Split(",");
                    Console.WriteLine("Commands supported by bound device:");
                    foreach (var c in supportedCommands)
                    {
                        Console.WriteLine(c);
                    }
                    return true;
                }
                Console.WriteLine(response);
                return true;
            }
            return false;
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