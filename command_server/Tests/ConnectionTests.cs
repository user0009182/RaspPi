using Protocol;
using Server;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ConnectionTests : IDisposable
    {
        Server.Server server;

        //TODO figure out why server ports have to be different in each test
        [Fact]
        public void NonTlsClientConnect()
        {
            var serverLog = new Logger();
            server = new Server.Server("server", null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, new TlsInfo(false, "", ""), Guid.NewGuid());
            Assert.Contains(serverLog.Data, l => l.Contains("server starting"));
            Assert.Contains(serverLog.Data, l => l.Contains("listening on port"));
            Assert.Contains(serverLog.Data, l => l.Contains("client connected"));
            Assert.Contains(serverLog.Data, l => l.Contains("handshake with connecting device complete"));
            Assert.Contains(clientLog.Data, l => l.Contains("connected to"));
            Assert.Contains(clientLog.Data, l => l.Contains("handshake with server complete"));
            Assert.Single(server.GetConnectedDevices());
        }

        [Fact]
        public void TlsClientConnect()
        {
            var serverLog = new Logger();
            server = new Server.Server("server", new TlsInfo(true, @"E:\git\tls\certificates\servercert.pem", @"E:\git\tls\certificates\serverkey.pem"), serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, new TlsInfo(true, @"E:\git\tls\certificates\clientcert.pem", @"E:\git\tls\certificates\clientkey.pem"), Guid.NewGuid());

            Assert.Contains(serverLog.Data, l => l.Contains("server starting"));
            Assert.Contains(serverLog.Data, l => l.Contains("listening on port"));
            Assert.Contains(serverLog.Data, l => l.Contains("client connected"));
            Assert.Contains(serverLog.Data, l => l.Contains("TLS authentication OK"));
            Assert.Contains(serverLog.Data, l => l.Contains("handshake with connecting device complete"));
            Assert.Contains(clientLog.Data, l => l.Contains("connected to"));
            Assert.Contains(clientLog.Data, l => l.Contains("TLS authenticating as client"));
            Assert.Contains(clientLog.Data, l => l.Contains("TLS authenticated OK"));
            Assert.Contains(clientLog.Data, l => l.Contains("handshake with server complete"));
            Assert.Single(server.GetConnectedDevices());
        }

        [Fact]
        public void MultiClientConnect()
        {
            var serverLog = new Logger();
            server = new Server.Server("server", null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            for (int i=0;i<5;i++)
            {
                var client = new DeviceClient("device1", clientLog);
                client.Connect("localhost", 10001, new TlsInfo(false, "", ""), Guid.NewGuid());
            }
            Assert.Equal(5, server.GetConnectedDevices().Count);
        }

        [Fact]
        public void RepeatedConnectDisconnect()
        {
            var serverLog = new Logger();
            server = new Server.Server("server", null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var clientId = new Guid("22345678-1234-1234-1234-123456789012");
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, null, clientId);
            client.Close();
            //Task.Delay(5000).Wait();
            client.Connect("localhost", 10001, null, clientId);
            client.Close();
            client.Connect("localhost", 10001, null, clientId);
            //TODO test messages
            Assert.Equal(1, server.GetConnectedDevices().Count);
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.Shutdown();
            }
        }
    }
}
