using Protocol;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class DeviceClientHandler
    {
        BlockingCollection<BaseMessage> sendQueue = new BlockingCollection<BaseMessage>();
        DeviceClient client;
        BlockingCollection<BaseMessage> receiveQueue;
        Server server;
        bool shutdown;
        CancellationToken cancellationToken;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();

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

        public DeviceClientHandler(DeviceClient client, int sessionId, Server server, BlockingCollection<BaseMessage> receiveQueue)
        {
            this.client = client;
            this.receiveQueue = receiveQueue;
            SessionId = sessionId;
            this.server = server;
            shutdown = false;
            cancellationToken = cancellationSource.Token;
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

        bool IsSocketTimeoutException(Exception e)
        {
            if (!(e is IOException))
                return false;
            var socketException = e.InnerException as SocketException;
            if (socketException == null)
                return false;
            return socketException.SocketErrorCode == SocketError.TimedOut;
        }

        void RecvThread()
        {
            while (!shutdown)
            {
                try
                {
                    var message = client.Reader.ReceiveMessage();
                    //tag the message with the ID of the device it was sent from
                    message.SourceDeviceId = client.RemoteDeviceId;
                    receiveQueue.Add(message);
                }
                catch (Exception e)
                {
                    if (IsSocketTimeoutException(e))
                    {
                        continue;
                    }
                    //TODO

                    //if (!client.IsConnected)
                    //{
                    //    server.Logger.Log("Connection closed remotely");
                    //    server.OnHandlerFault(this);
                    //    break;
                    //}

                    server.Logger.Log("Error receiving message");
                    server.OnHandlerFault(this);
                    break;
                }
            }
        }

        void SendThread()
        {
            try
            {
                //wait a message to send
                while (!shutdown)
                {
                    var message = sendQueue.Take(cancellationToken);
                    client.Writer.SendMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                //cancellationToken cancelled
            }
            catch (Exception e)
            {
                server.Logger.Log("Error sending message");
                server.OnHandlerFault(this);
            }
        }

        internal void Shutdown()
        {
            this.shutdown = true;
            client.Close();
            cancellationSource.Cancel();
        }
    }
}
