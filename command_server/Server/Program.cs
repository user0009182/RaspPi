using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static int boundSessionId=-1;
        static ConnectorRepository connectors = new ConnectorRepository();
        static void Main(string[] args)
        {
            var server = new DeviceServer(null, new Logger());
            server.Start(21008);
            
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine().ToLower();
                //ProcessCommand(server, command);
            }
        }

        static void OpenConnectionToRestServer(string hostname, int port)
        {
            
        }

        //public static bool ProcessCommand(DeviceServer server, string command)
        //{
        //    if (command == "list")
        //    {
        //        ListDevices(server);
        //        return true;
        //    }
        //    if (command.StartsWith("bind "))
        //    {
        //        var parts = command.Split(' ');
        //        int sessionId;
        //        if (!int.TryParse(parts[1], out sessionId))
        //        {
        //            Console.WriteLine("Invalid session Id");
        //            return true;
        //        }
        //        var device = server.GetConnectedDevice(sessionId);
        //        if (device != null)
        //        {
        //            boundSessionId = sessionId;
        //            Console.WriteLine("Bound to device " + boundSessionId);
        //        }
        //        else
        //        {
        //            Console.WriteLine("Connected device with session ID " + sessionId + " not found");
        //        }
        //        return true;
        //    }
        //    if (boundSessionId == -1)
        //    {
        //        Console.WriteLine("Not bound to a device");
        //        return true;
        //    }
        //    var response = server.SendCommand(boundSessionId, command);
        //    if (response == null)
        //    {
        //        Console.WriteLine("no response");
        //        return true;
        //    }
        //    else
        //    {
        //        if (command == "?")
        //        {
        //            var supportedCommands=response.Split(",");
        //            Console.WriteLine("Commands supported by bound device:");
        //            foreach (var c in supportedCommands)
        //            {
        //                Console.WriteLine(c);
        //            }
        //            return true;
        //        }
        //        if (command == "get_image")
        //        {
        //            System.IO.File.WriteAllBytes(@"imageA.jpg", Convert.FromBase64String(response));
        //        }
        //        Console.WriteLine(response);
        //        return true;
        //    }
        //}

        static void ListDevices(DeviceServer server)
        {
            var devices = server.GetConnectedDevices();
            Console.WriteLine($"{devices.Count} connected devices");
            if (devices.Count == 0)
                return;
            foreach (var device in server.GetConnectedDevices())
            {
                Console.WriteLine($"{device.DeviceId}) {device.IpAddress}");
            }
        }
    }
}