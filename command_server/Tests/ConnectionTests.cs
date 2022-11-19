using Server;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ConnectionTests
    {
        //TODO figure out why server ports have to be different in each test
        [Fact]
        public void NonTlsClientConnect()
        {
            var serverLog = new Logger();
            var server = new DeviceServer(null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10001, new TlsInfo(false, "",""), Guid.NewGuid());
            Assert.Contains(serverLog.Data, l => l.Contains("server starting"));
            Assert.Contains(serverLog.Data, l => l.Contains("listening on port"));
            Assert.Contains(serverLog.Data, l => l.Contains("client connected"));
            Assert.Contains(serverLog.Data, l => l.Contains("handshake complete OK"));
            Assert.Contains(clientLog.Data, l => l.Contains("connected to"));
            Assert.Contains(clientLog.Data, l => l.Contains("handshake complete OK"));
            Assert.Single(server.GetConnectedDevices());
        }

        [Fact]
        public void TlsClientConnect()
        {
            var serverLog = new Logger();
            var server = new DeviceServer(new TlsInfo(true, @"E:\git\tls\certificates\servercert.pem", @"E:\git\tls\certificates\serverkey.pem"), serverLog);
            server.Start(10002);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10002, new TlsInfo(true, @"E:\git\tls\certificates\clientcert.pem", @"E:\git\tls\certificates\clientkey.pem"), Guid.NewGuid());
            
            Assert.Contains(serverLog.Data, l => l.Contains("server starting"));
            Assert.Contains(serverLog.Data, l => l.Contains("listening on port"));
            Assert.Contains(serverLog.Data, l => l.Contains("client connected"));
            Assert.Contains(serverLog.Data, l => l.Contains("TLS authentication OK"));
            Assert.Contains(serverLog.Data, l => l.Contains("handshake complete OK"));
            Assert.Contains(clientLog.Data, l => l.Contains("connected to"));
            Assert.Contains(clientLog.Data, l => l.Contains("TLS authenticating as client"));
            Assert.Contains(clientLog.Data, l => l.Contains("TLS authenticated OK"));
            Assert.Contains(clientLog.Data, l => l.Contains("handshake complete OK"));
            Assert.Single(server.GetConnectedDevices());
        }

        [Fact]
        public void MultiClientConnect()
        {
            var serverLog = new Logger();
            var server = new DeviceServer(null, serverLog);
            server.Start(10003);
            var clientLog = new Logger();
            for (int i=0;i<5;i++)
            {
                var client = new DeviceClient(clientLog);
                client.Connect("localhost", 10003, new TlsInfo(false, "", ""), Guid.NewGuid());
            }
            Assert.Equal(5, server.GetConnectedDevices().Count);
        }
    }
}
