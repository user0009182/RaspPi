﻿using Protocol;
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
        Server server;
        TlsInfo tlsInfo;
        public DeviceListener(int listenPort, Server server, TlsInfo tlsInfo)
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
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
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
                    DeviceClient client = new DeviceClient(server.Name, server.Logger);
                    client.SetIdleTimeoutPolicy(10, true);
                    client.WrapTcpClient(tcpClient, tlsInfo);
                    server.Logger.Log($"begin handshake with connecting device");
                    var handshake = new DeviceHandshake(server.Logger);
                    var success = handshake.DoHandshakeAsServer(client, server.DeviceId);
                    if (!success)
                    {
                        server.WriteLog("handshake with connecting device failed");
                        client.Close();
                        return;
                    }
                    server.WriteLog("handshake with connecting device complete");
                    server.WriteLog($"{client.RemoteName} {client.RemoteDeviceId} {client.RemoteEndpoint}");
                    var handler = server.CreateDeviceClientHandler(client);
                    return;
                }
                catch (Exception)
                {
                    server.WriteLog("exception accepting connecting device");
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
