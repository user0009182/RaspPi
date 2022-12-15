using Protocol;
using Hub;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ServerCommandTests : IDisposable
    {
        Hub.Hub server;
        [Fact]
        public void RequestedDeviceNotFound()
        {
            var unknownDeviceId = Guid.Parse("{64648a8a-616a-48f8-92f2-5014df076c56}");

            var serverLog = new TestTraceSink();
            server = new Hub.Hub("server", null, serverLog);
            server.Start(10001);
            var clientLog = new TestTraceSink();
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, new TlsInfo(false, "", ""), Guid.NewGuid());
            client.Writer.SendMessage(new RequestMessage(1, null, unknownDeviceId, Encoding.ASCII.GetBytes("hello")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("notfound", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerPingCommand()
        {
            var serverLog = new TestTraceSink();
            server = new Hub.Hub("server", null, serverLog);
            server.Start(10001);
            var clientLog = new TestTraceSink();
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, null, Guid.NewGuid());
            client.Writer.SendMessage(new RequestMessage(1, null, server.DeviceId, Encoding.ASCII.GetBytes("ping")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("pong", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerUnknownCommand()
        {
            var serverLog = new TestTraceSink();
            server = new Hub.Hub("server", null, serverLog);
            server.Start(10001);
            var clientLog = new TestTraceSink();
            var client = new DeviceClient("device1", clientLog);
            client.Connect("localhost", 10001, new TlsInfo(false, "", ""), Guid.NewGuid());
            client.Writer.SendMessage(new RequestMessage(1, null, server.DeviceId, Encoding.ASCII.GetBytes("abbcdefg")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("unknown command", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void Routing()
        {
            //connect a "device" and a "terminal" client to the server
            //send a message from the terminal targetted to the device
            //the device expects to receive the message and sends a response
            //the terminal expects to receive the response
            var serverLog = new TestTraceSink();
            server = new Hub.Hub("server", null, serverLog);
            server.Start(10001);
            var clientLog = new TestTraceSink();
            var client1Guid = new Guid("12345678-1234-1234-1234-123456789012");
            var device = new DeviceClient("device1", clientLog);
            device.Connect("localhost", 10001, null, client1Guid);
            var client2Guid = new Guid("12345678-1234-1234-1234-123456789013");
            var terminal = new DeviceClient("device2", clientLog);
            terminal.Connect("localhost", 10001, null, client2Guid);
            //Task.Delay(2000).Wait();
            terminal.Writer.SendMessage(new RequestMessage(1, "device1", Guid.Empty, Encoding.ASCII.GetBytes("hello")));
            var request = (RequestMessage)device.Reader.ReceiveMessage();
            Assert.Equal("hello", Encoding.ASCII.GetString(request.RequestData));
            device.Writer.SendMessage(new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes("hi im device1")));
            var response = (ResponseMessage)terminal.Reader.ReceiveMessage();
            Assert.Equal("hi im device1", Encoding.ASCII.GetString(response.RequestData));
        }


        [Fact]
        public void Forwarding()
        {
            //set up a "device1" connected to a server "server"
            //set up a "relay" server connected to the "server"
            //configure the relay server to forward all requests to "server"
            //connect "terminal" to the relay server and send a request to device1
            //verify request and response are routed correctly
            var relayLog = new TestTraceSink();
            var relayServer = new Hub.Hub("relay", null, relayLog);
            relayServer.Start(10001);
            relayServer.Router.ForwardServerName = "server";
            var serverLog = new TestTraceSink();
            var server = new Hub.Hub("server", null, serverLog);
            server.Start(10002);
            server.OutgoingConnectionProcessor.RegisterOutgoingConnection("localhost", 10001, null);
            var device1Log = new TestTraceSink();
            var device1Guid = new Guid("12345678-1234-1234-1234-123456789012");
            var device1 = new DeviceClient("device1", device1Log);
            device1.Connect("localhost", 10002, null, device1Guid);
            var terminalLog = new TestTraceSink();
            var terminalGuid = new Guid("12345678-1234-1234-1234-123456789013");
            var terminal = new DeviceClient("terminal", terminalLog);
            terminal.Connect("localhost", 10001, null, terminalGuid);
            terminal.Writer.SendMessage(new RequestMessage(1, "device1", Guid.Empty, Encoding.ASCII.GetBytes("hello")));
            var request = (RequestMessage)device1.Reader.ReceiveMessage();
            Assert.Equal("hello", Encoding.ASCII.GetString(request.RequestData));
            device1.Writer.SendMessage(new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes("hi im device1")));
            var response = (ResponseMessage)terminal.Reader.ReceiveMessage();
            Assert.Equal("hi im device1", Encoding.ASCII.GetString(response.RequestData));
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
