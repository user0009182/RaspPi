using Protocol;
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
    public class Server
    {
        DeviceListener listener;
        int nextSessionId = 0;
        BlockingCollection<BaseMessage> receivedMessageQueue = new BlockingCollection<BaseMessage>();
        Dictionary<Guid, DeviceClientHandler> connectedDevices = new Dictionary<Guid, DeviceClientHandler>();
        IncomingMessageProcessor incomingMessageProcessor;
        ResponseTimeoutThread responseTimeoutThread;
        ServerCommandHandler commandHandler;
        public RoutedRequestTable RoutedRequestTable { get; }

        ILogger logger;
        TlsInfo tlsInfo;
        public ILogger Logger
        {
            get
            {
                return logger;
            }
        }

        public MessageRouter Router { get; }
        public Guid DeviceId { get; }
        public BlockingCollection<RequestMessage> CommandQueue { get; internal set; } = new BlockingCollection<RequestMessage>();

        public Server(TlsInfo tlsInfo, ILogger logger)
        {
            if (tlsInfo == null)
                tlsInfo = new TlsInfo(false, "", "");
            this.tlsInfo = tlsInfo;
            this.logger = logger;
            DeviceId = Guid.NewGuid();
            Router = new MessageRouter(this);
            RoutedRequestTable = new RoutedRequestTable(this, logger);
            incomingMessageProcessor = new IncomingMessageProcessor(this, receivedMessageQueue, logger);
            responseTimeoutThread = new ResponseTimeoutThread(this, RoutedRequestTable, logger);
            commandHandler = new ServerCommandHandler(this, logger);
            incomingMessageProcessor.Start();
            responseTimeoutThread.Start();
            commandHandler.Start();
        }

        public void CreateDeviceClientHandler(DeviceClient client)
        {
            lock (connectedDevices)
            {
                var handler = new DeviceClientHandler(client, nextSessionId, this, receivedMessageQueue);
                handler.Start();
                connectedDevices.Add(client.RemoteDeviceId, handler);
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
                    ret.Add(new ConnectedDeviceInfo(entry.Value.Client.Name, entry.Key, entry.Value.RemoteEndpoint.ToString()));
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

        internal DeviceClientHandler GetConnectedDevice(Guid deviceId)
        {
            DeviceClientHandler ret = null;
            lock (connectedDevices)
            {
                connectedDevices.TryGetValue(deviceId, out ret);
            }
            return ret;
        }

        uint nextRequestId = 0;
        object lock_obj = new object();
        internal uint GenerateRequestId()
        {
            lock(lock_obj)
            {
                nextRequestId++;
                return nextRequestId;
            }
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
                connectedDevices.Remove(clientHandler.Client.RemoteDeviceId);
            }
            WriteLog($"device {clientHandler.SessionId} deregistered");
        }

        public void WriteLog(string text)
        {
            logger.Log(text);
        }
    }

    public class ConnectedDeviceInfo
    {
        public Guid DeviceId;
        public string IpAddress;
        public string Name;

        public ConnectedDeviceInfo(string name, Guid deviceId, string ipAddress)
        {
            Name = name;
            DeviceId = deviceId;
            IpAddress = ipAddress;
        }
    }
}