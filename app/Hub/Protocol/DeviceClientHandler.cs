using Protocol;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hub
{
    //public class DeviceClientHandler
    //{
    //    BlockingCollection<BaseMessage> sendQueue = new BlockingCollection<BaseMessage>();
    //    DeviceClient client;
    //    BlockingCollection<BaseMessage> receiveQueue;
    //    //Hub server;
    //    bool shutdown;
    //    CancellationToken cancellationToken;
    //    CancellationTokenSource cancellationSource = new CancellationTokenSource();
    //    DateTime lastRemoteContact = DateTime.MinValue;
    //    EventTracer tracer;

    //    public DeviceClient Client
    //    {
    //        get
    //        {
    //            return client;
    //        }
    //    }

    //    public BlockingCollection<BaseMessage> SendQueue
    //    {
    //        get
    //        {
    //            return sendQueue;
    //        }
    //    }

    //    public DeviceClientHandler(DeviceClient client, BlockingCollection<BaseMessage> receiveQueue, EventTracer tracer)
    //    {
    //        this.client = client;
    //        this.receiveQueue = receiveQueue;
    //        shutdown = false;
    //        cancellationToken = cancellationSource.Token;
    //        this.tracer = tracer;
    //    }

    //    public EndPoint RemoteEndpoint
    //    {
    //        get
    //        {
    //            return client.RemoteEndpoint;
    //        }
    //    }

    //    public void Start()
    //    {
    //        lastRemoteContact = DateTime.Now;
    //        Task.Run(() => RecvThread());
    //        Task.Run(() => SendThread());
    //    }

    //    void RecvThread()
    //    {
    //        while (!shutdown)
    //        {
    //            try
    //            {
    //                var message = client.Reader.ReceiveMessage();
    //                lastRemoteContact = DateTime.Now;
    //                if (message.Type == DeviceProtocolMessageType.KeepAlive)
    //                {
    //                    tracer.Debug(TraceEventId.ReceivedKeepaliveResponse);
    //                    //consume keepalives
    //                    continue;
    //                }
    //                //tag the message with the ID of the device it was sent from
    //                message.SourceDeviceId = client.RemoteDeviceId;
    //                receiveQueue.Add(message);
    //            }
    //            catch (Exception e)
    //            {
    //                if (DeviceProtocolException.IsSocketTimeoutException(e))
    //                {
    //                    if (client.IdleTimeoutPolicy.SendKeepaliveResponses)
    //                    {
    //                        if (DateTime.Now.Subtract(lastRemoteContact).TotalSeconds > client.IdleTimeoutPolicy.Interval)
    //                        {
    //                            tracer.Failure(TraceEventId.IdleTimeoutTriggered);
    //                            Client.RaiseFailure();
    //                        }
    //                    }
    //                    continue;
    //                }
    //                //TODO

    //                //if (!client.IsConnected)
    //                //{
    //                //    server.Logger.Log("Connection closed remotely");
    //                //    server.OnHandlerFault(this);
    //                //    break;
    //                //}

    //                tracer.Failure(TraceEventId.ClientMessageReceiveError, e.ToString());
    //                Client.RaiseFailure();
    //                break;
    //            }
    //        }
    //    }

    //    bool SendKeepAlives
    //    {
    //        get
    //        {
    //            return KeepAliveInterval > 0;
    //        }
    //    }

    //    int KeepAliveInterval
    //    {
    //        get
    //        {
    //            return client.IdleTimeoutPolicy.Interval / 2 + 1;
    //        }
    //    }

    //    DateTime nextKeepAlive;
    //    int SecondsUntilNextKeepalive()
    //    {
    //        if (!SendKeepAlives)
    //        {
    //            return -1; //infinite
    //        }
    //        return Math.Max(0, (int)nextKeepAlive.Subtract(DateTime.Now).TotalSeconds);
    //    }

    //    void SendThread()
    //    {
    //        if (SendKeepAlives)
    //        {
    //            nextKeepAlive = DateTime.Now.AddSeconds(KeepAliveInterval);
    //        }
    //        try
    //        {
    //            //wait a message to send
    //            while (!shutdown)
    //            {
    //                BaseMessage message;
    //                int waitTime = SecondsUntilNextKeepalive() * 1000;
    //                var messageReceived = sendQueue.TryTake(out message, waitTime, cancellationToken);
    //                if (messageReceived)
    //                {
    //                    client.Writer.SendMessage(message);
    //                }
                    
    //                if (SendKeepAlives && SecondsUntilNextKeepalive() < 1)
    //                {
    //                    SendQueue.Add(new KeepAliveMessage());
    //                    nextKeepAlive = DateTime.Now.AddSeconds(KeepAliveInterval);
    //                }
    //            }
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            //cancellationToken cancelled
    //        }
    //        catch (Exception e)
    //        {
    //            tracer.Failure(TraceEventId.ClientMessageSendError, e.ToString());
    //            Client.RaiseFailure();
    //        }
    //    }

    //    internal void Shutdown()
    //    {
    //        this.shutdown = true;
    //        client.Close();
    //        cancellationSource.Cancel();
    //    }
    //}
}
