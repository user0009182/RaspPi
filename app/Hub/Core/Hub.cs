using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hub
{
    /// <summary>
    /// A Hub is a listening TCP server that devices can connect to
    /// It performs message routing between connected devices
    /// It also acts like a device itself - a hub can be connected to another hub or sent commands from a connected device
    /// </summary>
    public class Hub
    {
        DeviceListener listener;
        int nextSessionId = 0;
        BlockingCollection<BaseMessage> receivedMessageQueue = new BlockingCollection<BaseMessage>();
        Dictionary<Guid, DeviceClient> connectedDevices = new Dictionary<Guid, DeviceClient>();
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

        public Hub(string name, TlsInfo tlsInfo, ITraceSink traceSink)
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

        public void StartDeviceClientHandler(DeviceClient client)
        {
            lock (connectedDevices)
            {
                client.StartHandler(OnClientReceiveMessage, OnHandlerFault);
                connectedDevices.Add(client.RemoteDeviceId, client);
            }
        }

        void OnClientReceiveMessage(BaseMessage message)
        {
            receivedMessageQueue.Add(message);
        }

        public List<ConnectedDeviceInfo> GetConnectedDevices()
        {
            var ret = new List<ConnectedDeviceInfo>();
            lock(connectedDevices)
            {
                foreach (var entry in connectedDevices)
                {
                    ret.Add(new ConnectedDeviceInfo(entry.Value.RemoteName, entry.Key, entry.Value.RemoteEndpoint.ToString()));
                }
            }
            return ret;
        }

        internal DeviceClient GetDeviceByIp(string ip)
        {
            lock(connectedDevices)
            {
                return connectedDevices.Values.FirstOrDefault(c => c.RemoteEndpoint.ToString() == ip);
            }
        }

        internal DeviceClient GetConnectedDevice(Guid deviceId)
        {
            DeviceClient ret = null;
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

        internal void OnHandlerFault(DeviceClient client)
        {
            Trace.Failure(TraceEventId.HandlerFault, client.RemoteName);
            client.Close();
            OutgoingConnectionProcessor.OnDisconnect(client);
            RemoveConnectedClient(client);
        }

        public void Start(int listenPort)
        {
            Trace.Flow(TraceEventId.ServerStarting);
            listener = new DeviceListener(listenPort, this, tlsInfo);
            listener.Start();
        }

        internal void RemoveConnectedClient(DeviceClient client)
        {
            lock (connectedDevices)
            {
                connectedDevices.Remove(client.RemoteDeviceId);
            }
            Trace.Flow(TraceEventId.DeviceDeregistered, client.RemoteName);
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