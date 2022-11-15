using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    public class DeviceServer
    {
        DeviceListener listener;
        int nextSessionId = 0;
        BlockingCollection<BaseMessage> receivedMessageQueue = new BlockingCollection<BaseMessage>();
        Dictionary<int, DeviceClientHandler> connectedDevices = new Dictionary<int, DeviceClientHandler>();
        ILogger logger;
        TlsInfo tlsInfo;

        public ILogger Logger
        {
            get
            {
                return logger;
            }
        }

        public DeviceServer(TlsInfo tlsInfo, ILogger logger)
        {
            if (tlsInfo == null)
                tlsInfo = new TlsInfo(false, "", "");
            this.tlsInfo = tlsInfo;
            this.logger = logger;
        }

        public void CreateDeviceClientHandler(DeviceClient client)
        {
            lock (connectedDevices)
            {
                var handler = new DeviceClientHandler(client, nextSessionId, this, receivedMessageQueue);
                handler.Start();
                connectedDevices.Add(nextSessionId, handler);
                nextSessionId++;
            }
        }

        public List<ConnectedDeviceInfo> GetConnectedDevices()
        {
            var ret = new List<ConnectedDeviceInfo>();
            lock(connectedDevices)
            {
                foreach (var entry in connectedDevices)
                {
                    ret.Add(new ConnectedDeviceInfo(entry.Key, entry.Value.RemoteEndpoint.ToString()));
                }
            }
            return ret;
        }

        internal DeviceClientHandler GetDeviceByIp(string ip)
        {
            lock(connectedDevices)
            {
                return connectedDevices.Values.FirstOrDefault(c => c.RemoteEndpoint.ToString() == ip);
            }
        }

        internal DeviceClientHandler GetConnectedDevice(int sessionId)
        {
            DeviceClientHandler ret = null;
            lock (connectedDevices)
            {
                connectedDevices.TryGetValue(sessionId, out ret);
            }
            return ret;
        }

        internal void OnHandlerFault(DeviceClientHandler handler)
        {
            WriteLog($"closing session {handler.SessionId} - fault detected");
            handler.Shutdown();
            RemoveConnectedClient(handler);
        }

        public void Start(int listenPort)
        {
            WriteLog("server starting");
            listener = new DeviceListener(listenPort, this, tlsInfo);
            listener.Start();
        }

        internal void RemoveConnectedClient(DeviceClientHandler clientHandler)
        {
            lock (connectedDevices)
            {
                connectedDevices.Remove(clientHandler.SessionId);
            }
            WriteLog($"device {clientHandler.SessionId} deregistered");
        }

        internal string SendCommand(int sessionId, string command)
        {
            var client = GetConnectedDevice(sessionId);
            if (client == null)
            {
                WriteLog($"Bound device {sessionId} is not connected");
                return null;
            }
            return ""; // client.EnqueueCommand(command);
        }

        public void WriteLog(string text)
        {
            logger.Log(text);
        }

        //void ListenerThread(int listenPort)
        //{
        //    var listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
        //    listener.Start(10);
        //    Console.WriteLine($"Listening on port {listenPort}");
        //    while (true)
        //    {
        //        var client = listener.AcceptTcpClient();
        //        Debug.WriteLine($"client connected {client.Client.RemoteEndPoint}");
        //        var handler = new ClientHandler(client, this);
        //        Task.Run(() => handler.Run()); // RunClient(client));
        //    }
        //}
    }

    public struct ConnectedDeviceInfo
    {
        public int SessionId;
        public string IpAddress;

        public ConnectedDeviceInfo(int sessionId, string ipAddress)
        {
            SessionId = sessionId;
            IpAddress = ipAddress;
        }
    }
}