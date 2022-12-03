using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public OutgoingConnectionProcessor OutgoingConnectionProcessor { get; }

        TlsInfo tlsInfo;
        public EventTracer Trace { get; private set; }

        public MessageRouter Router { get; }
        public Guid DeviceId { get; }
        public BlockingCollection<RequestMessage> CommandQueue { get; internal set; } = new BlockingCollection<RequestMessage>();
        public string Name { get; }

        public Server(string name, TlsInfo tlsInfo, ITraceSink traceSink)
        {
            Name = name;
            if (tlsInfo == null)
                tlsInfo = new TlsInfo(false, "", "");
            this.tlsInfo = tlsInfo;
            Trace = new EventTracer(traceSink);
            DeviceId = Guid.NewGuid();
            Router = new MessageRouter(this);
            RoutedRequestTable = new RoutedRequestTable(this, Trace);
            incomingMessageProcessor = new IncomingMessageProcessor(this, receivedMessageQueue, Trace);
            responseTimeoutThread = new ResponseTimeoutThread(this, RoutedRequestTable, Trace);
            commandHandler = new ServerCommandHandler(this, Trace);
            OutgoingConnectionProcessor = new OutgoingConnectionProcessor(this, Trace);
            incomingMessageProcessor.Start();
            responseTimeoutThread.Start();
            commandHandler.Start();
            OutgoingConnectionProcessor.Start();
        }

        public void Shutdown()
        {
            listener.Stop();
        }

        public DeviceClientHandler CreateDeviceClientHandler(DeviceClient client)
        {
            lock (connectedDevices)
            {
                var handler = new DeviceClientHandler(client, nextSessionId, this, receivedMessageQueue);
                handler.Start();
                connectedDevices.Add(client.RemoteDeviceId, handler);
                nextSessionId++;
                return handler;
            }
        }

        public List<ConnectedDeviceInfo> GetConnectedDevices()
        {
            var ret = new List<ConnectedDeviceInfo>();
            lock(connectedDevices)
            {
                foreach (var entry in connectedDevices)
                {
                    ret.Add(new ConnectedDeviceInfo(entry.Value.Client.RemoteName, entry.Key, entry.Value.RemoteEndpoint.ToString()));
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
            Trace.Failure(TraceEventId.HandlerFault, handler.Client.RemoteName);
            handler.Shutdown();
            OutgoingConnectionProcessor.OnDisconnect(handler);
            RemoveConnectedClient(handler);
        }

        public void Start(int listenPort)
        {
            Trace.Flow(TraceEventId.ServerStarting);
            listener = new DeviceListener(listenPort, this, tlsInfo);
            listener.Start();
        }

        internal void RemoveConnectedClient(DeviceClientHandler clientHandler)
        {
            lock (connectedDevices)
            {
                connectedDevices.Remove(clientHandler.Client.RemoteDeviceId);
            }
            Trace.Flow(TraceEventId.DeviceDeregistered, clientHandler.Client.RemoteName);
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