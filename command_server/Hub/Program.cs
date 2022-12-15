using Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace Hub
{
    class Program
    {
        static void Main(string[] args)
        {
            int defaultListenPort = 21008;
            int listenPort = args.Length > 0 ? Convert.ToInt32(args[0]) : defaultListenPort;

            TlsInfo tlsInfo = null; // new TlsInfo(true, @"E:\git\tls\certificates\servercert.pem", @"E:\git\tls\certificates\serverkey.pem");
            var server = new Hub("server", tlsInfo, new ServerTraceSink());

            //if (listenPort == 21008)
            //{
            //    server.OutgoingConnectionProcessor.RegisterOutgoingConnection("localhost", 21007, new TlsInfo(true, @"E:\git\tls\certificates\clientcert.pem", @"E:\git\tls\certificates\clientkey.pem"));
            //}

            server.Start(listenPort);
            
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine().ToLower();
                if (command == "exit")
                    break;
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

        static void ListDevices(Hub server)
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

    class ServerTraceSink : ITraceSink
    {
        public bool IsEventTypeTraced(Protocol.TraceEventType type)
        {
            return true;
        }

        public void OnEvent(TraceEvent e)
        {
            Console.WriteLine(e.Id);
        }
    }
}