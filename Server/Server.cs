using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        List<ClientHandler> connectedClients = new List<ClientHandler>();

        public void RegisterConnectedClient(ClientHandler handler)
        {
            lock(connectedClients)
            {
                connectedClients.Add(handler);
            }
            Debug.WriteLine("device registered");
        }

        internal List<string> GetConnectedDevices()
        {
            var ret = new List<string>();
            lock(connectedClients)
            {
                foreach (var clientHandler in connectedClients)
                {
                    ret.Add(clientHandler.RemoteEndpoint.ToString());
                }
            }
            return ret;
        }

        internal ClientHandler GetDevice(string ip)
        {
            lock(connectedClients)
            {
                return connectedClients.FirstOrDefault(c => c.RemoteEndpoint.ToString() == ip);
            }
        }

        public void Start(int listenPort)
        {
            Task.Run(() => ListenerThread(listenPort));
        }

        ClientHandler GetClient(int i)
        {
            lock (connectedClients)
            {
                return connectedClients[i];
            }
        }

        internal void DeregisterClient(ClientHandler clientHandler)
        {
            lock (connectedClients)
            {
                connectedClients.Remove(clientHandler);
            }
            Debug.WriteLine("device deregistered");
        }

        internal string SendCommand(string ip, string command)
        {
            var client = GetDevice(ip);
            if (client == null)
            {
                Console.WriteLine($"Bound device {ip} is not connected");
                return null;
            }
            return client.EnqueueCommand(command);
        }

        void ListenerThread(int listenPort)
        {
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
            listener.Start(10);
            Console.WriteLine($"Listening on port {listenPort}");
            while (true)
            {
                var client = listener.AcceptTcpClient();
                Debug.WriteLine($"client connected {client.Client.RemoteEndPoint}");
                var handler = new ClientHandler(client, this);
                Task.Run(() => handler.Run()); // RunClient(client));
            }
        }
    }
}