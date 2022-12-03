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
    public class DeviceClientHandler
    {
        BlockingCollection<BaseMessage> sendQueue = new BlockingCollection<BaseMessage>();
        DeviceClient client;
        BlockingCollection<BaseMessage> receiveQueue;
        Server server;
        bool shutdown;
        CancellationToken cancellationToken;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();
        DateTime lastRemoteContact = DateTime.MinValue;

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

        void RecvThread()
        {
            while (!shutdown)
            {
                try
                {
                    var message = client.Reader.ReceiveMessage();
                    lastRemoteContact = DateTime.Now;
                    if (message.Type == DeviceProtocolMessageType.KeepAlive)
                    {
                        server.Trace.Debug(TraceEventId.ReceivedKeepaliveResponse);
                        //consume keepalives
                        continue;
                    }
                    //tag the message with the ID of the device it was sent from
                    message.SourceDeviceId = client.RemoteDeviceId;
                    receiveQueue.Add(message);
                }
                catch (Exception e)
                {
                    if (DeviceProtocolException.IsSocketTimeoutException(e))
                    {
                        if (client.IdleTimeoutPolicy.SendKeepaliveResponses)
                        {
                            if (DateTime.Now.Subtract(lastRemoteContact).TotalSeconds > client.IdleTimeoutPolicy.Interval)
                            {
                                server.Trace.Failure(TraceEventId.IdleTimeoutTriggered);
                                server.OnHandlerFault(this);
                            }
                        }
                        continue;
                    }
                    //TODO

                    //if (!client.IsConnected)
                    //{
                    //    server.Logger.Log("Connection closed remotely");
                    //    server.OnHandlerFault(this);
                    //    break;
                    //}

                    server.Trace.Failure(TraceEventId.ClientMessageReceiveError, e.ToString());
                    server.OnHandlerFault(this);
                    break;
                }
            }
        }

        bool SendKeepAlives
        {
            get
            {
                return KeepAliveInterval > 0;
            }
        }

        int KeepAliveInterval
        {
            get
            {
                return client.IdleTimeoutPolicy.Interval / 2 + 1;
            }
        }

        DateTime nextKeepAlive;
        int SecondsUntilNextKeepalive()
        {
            if (!SendKeepAlives)
            {
                return -1; //infinite
            }
            return Math.Max(0, (int)nextKeepAlive.Subtract(DateTime.Now).TotalSeconds);
        }

        void SendThread()
        {
            if (SendKeepAlives)
            {
                nextKeepAlive = DateTime.Now.AddSeconds(KeepAliveInterval);
            }
            try
            {
                //wait a message to send
                while (!shutdown)
                {
                    BaseMessage message;
                    int waitTime = SecondsUntilNextKeepalive() * 1000;
                    var messageReceived = sendQueue.TryTake(out message, waitTime, cancellationToken);
                    if (messageReceived)
                    {
                        client.Writer.SendMessage(message);
                    }
                    
                    if (SendKeepAlives && SecondsUntilNextKeepalive() < 1)
                    {
                        SendQueue.Add(new KeepAliveMessage());
                        nextKeepAlive = DateTime.Now.AddSeconds(KeepAliveInterval);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //cancellationToken cancelled
            }
            catch (Exception e)
            {
                server.Trace.Failure(TraceEventId.ClientMessageSendError, e.ToString());
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
