using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Hub
{
    class DeviceListener
    {
        int listenPort;
        Hub server;
        TlsInfo tlsInfo;
        public DeviceListener(int listenPort, Hub server, TlsInfo tlsInfo)
        {
            this.listenPort = listenPort;
            this.server = server;
            this.tlsInfo = tlsInfo;
        }

        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        TcpListener listener;
        void ThreadProc()
        {
            try
            {
                listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
                listener.Start(10);
                server.Trace.Flow(TraceEventId.ListenerStarted, Convert.ToString(listenPort));
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    server.Trace.Flow(TraceEventId.ClientConnecting, Convert.ToString(client.Client.RemoteEndPoint));
                    HandleClientAsync(client);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        void HandleClientAsync(TcpClient tcpClient)
        {
            Task.Run(() =>
            {
                try
                {
                    DeviceClient client = new DeviceClient(server.Name, server.Trace.Sink);
                    client.SetIdleTimeoutPolicy(60, true);
                    client.WrapTcpClient(tcpClient, tlsInfo);
                    server.Trace.Detail(TraceEventId.HandshakeAsServerBegin);
                    var handshake = new DeviceHandshake(server.Trace);
                    var success = handshake.DoHandshakeAsServer(client, server.DeviceId);
                    if (!success)
                    {
                        server.Trace.Failure(TraceEventId.HandshakeAsServerFailed);
                        client.Close();
                        return;
                    }
                    server.Trace.Detail(TraceEventId.HandshakeAsServerSuccess);
                    server.Trace.Flow(TraceEventId.ClientConnected, client.RemoteName, client.RemoteDeviceId.ToString(), client.RemoteEndpoint.ToString());
                    server.StartDeviceClientHandler(client);
                    return;
                }
                catch (Exception e)
                {
                    server.Trace.Error(TraceEventId.ClientAcceptError, e.ToString());
                }
            });
        }

        internal void Stop()
        {
            if (listener != null)
            {
                listener.Server.Close();
                listener.Stop();
            }
        }
    }
}
