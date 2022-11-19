using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class DeviceListener
    {
        int listenPort;
        DeviceServer server;
        TlsInfo tlsInfo;
        public DeviceListener(int listenPort, DeviceServer server, TlsInfo tlsInfo)
        {
            this.listenPort = listenPort;
            this.server = server;
            this.tlsInfo = tlsInfo;
        }

        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        void ThreadProc()
        {
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
            listener.Start(10);
            server.WriteLog($"listening on port {listenPort} tls={tlsInfo.UseTls}");
            while (true)
            {
                var client = listener.AcceptTcpClient();
                server.WriteLog($"client connected {client.Client.RemoteEndPoint}");
                HandleClientAsync(client);
            }
        }

        void HandleClientAsync(TcpClient tcpClient)
        {
            Task.Run(() =>
            {
                try
                {
                    DeviceClient client = new DeviceClient(server.Logger);
                    client.WrapTcpClient(tcpClient, tlsInfo);
                    var success = client.DoHandshakeAsServer(server);
                    if (!success)
                    {
                        server.WriteLog("incoming client device protocol handshake failed");
                        return null;
                    }
                    server.CreateDeviceClientHandler(client);
                    return client;
                }
                catch (Exception e)
                {
                    server.WriteLog("exception accepting client");
                    throw;
                }
            });
        }
    }
}
