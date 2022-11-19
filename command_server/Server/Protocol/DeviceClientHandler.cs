using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class DeviceClientHandler
    {
        BlockingCollection<BaseMessage> sendQueue = new BlockingCollection<BaseMessage>();
        DeviceClient client;
        BlockingCollection<BaseMessage> receiveQueue;
        DeviceServer server;
        bool shutdown;

        public DeviceClient Client
        {
            get
            {
                return client;
            }
        }

        public BlockingCollection<BaseMessage> SendQueue
        {
            get
            {
                return sendQueue;
            }
        }

        public DeviceClientHandler(DeviceClient client, int sessionId, DeviceServer server, BlockingCollection<BaseMessage> receiveQueue)
        {
            this.client = client;
            this.receiveQueue = receiveQueue;
            SessionId = sessionId;
            this.server = server;
            shutdown = false;
        }

        public EndPoint RemoteEndpoint
        {
            get
            {
                return client.RemoteEndpoint;
            }
        }

        public int SessionId { get; private set; }

        public void Start()
        {
            Task.Run(() => RecvThread());
            Task.Run(() => SendThread());
        }

        void RecvThread()
        {
            try
            {
                while (!shutdown)
                {
                    var message = client.Reader.ReceiveMessage();
                    //tag the message with the ID of the device it was sent from
                    message.SourceDeviceId = client.RemoteDeviceId;
                    receiveQueue.Add(message);

                }
            }
            catch (Exception)
            {
                server.OnHandlerFault(this);
            }
        }

        void SendThread()
        {
            try
            {
                //wait a message to send
                while (!shutdown)
                {
                    var message = sendQueue.Take();
                    client.Writer.SendMessage(message);
                }
            }
            catch (Exception)
            {
                server.OnHandlerFault(this);
            }
        }

        internal void Shutdown()
        {
            this.shutdown = true;
        }
    }
}
