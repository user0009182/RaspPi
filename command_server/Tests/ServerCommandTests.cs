using Protocol;
using Server;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ServerCommandTests : IDisposable
    {
        Server.Server server;

        //TODO figure out why server ports have to be different in each test
        [Fact]
        public void RequestedDeviceNotFound()
        {
            var unknownDeviceId = Guid.Parse("{64648a8a-616a-48f8-92f2-5014df076c56}");

            var serverLog = new Logger();
            server = new Server.Server(null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10001, new TlsInfo(false, "", ""), Guid.NewGuid());
            client.Writer.SendMessage(new RequestMessage(1, null, unknownDeviceId, Encoding.ASCII.GetBytes("hello")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("notfound", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerPingCommand()
        {
            var serverLog = new Logger();
            server = new Server.Server(null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10001, null, Guid.NewGuid());
            client.Writer.SendMessage(new RequestMessage(1, null, server.DeviceId, Encoding.ASCII.GetBytes("ping")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("pong", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerUnknownCommand()
        {
            var serverLog = new Logger();
            server = new Server.Server(null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
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
            var serverLog = new Logger();
            server = new Server.Server(null, serverLog);
            server.Start(10001);
            var clientLog = new Logger();
            var client1Guid = new Guid("12345678-1234-1234-1234-123456789012");
            var device = new DeviceClient(clientLog);
            device.AssignName("device1");
            device.Connect("localhost", 10001, null, client1Guid);
            var client2Guid = new Guid("12345678-1234-1234-1234-123456789013");
            var terminal = new DeviceClient(clientLog);
            terminal.AssignName("terminal");
            terminal.Connect("localhost", 10001, null, client2Guid);
            //Task.Delay(2000).Wait();
            terminal.Writer.SendMessage(new RequestMessage(1, "device1", Guid.Empty, Encoding.ASCII.GetBytes("hello")));
            var request = (RequestMessage)device.Reader.ReceiveMessage();
            Assert.Equal("hello", Encoding.ASCII.GetString(request.RequestData));
            device.Writer.SendMessage(new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes("hi im device1")));
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
