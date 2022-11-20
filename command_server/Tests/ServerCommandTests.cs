using Protocol;
using Server;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ServerCommandTests
    {
        //TODO figure out why server ports have to be different in each test
        [Fact]
        public void RequestedDeviceNotFound()
        {
            var unknownDeviceId = Guid.Parse("{64648a8a-616a-48f8-92f2-5014df076c56}");

            var serverLog = new Logger();
            var server = new Server.Server(null, serverLog);
            server.Start(10004);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10004, new TlsInfo(false, "", ""), Guid.NewGuid());
            Task.Delay(5000).Wait();
            client.Writer.SendMessage(new RequestMessage(1, null, unknownDeviceId, Encoding.ASCII.GetBytes("hello")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("notfound", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerPingCommand()
        {
            var serverLog = new Logger();
            var server = new Server.Server(null, serverLog);
            server.Start(10005);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10005, new TlsInfo(false, "", ""), Guid.NewGuid());
            Task.Delay(5000).Wait();
            client.Writer.SendMessage(new RequestMessage(1, null, server.DeviceId, Encoding.ASCII.GetBytes("ping")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("pong", Encoding.ASCII.GetString(response.RequestData));
        }

        [Fact]
        public void ServerUnknownCommand()
        {
            var serverLog = new Logger();
            var server = new Server.Server(null, serverLog);
            server.Start(10006);
            var clientLog = new Logger();
            var client = new DeviceClient(clientLog);
            client.Connect("localhost", 10006, new TlsInfo(false, "", ""), Guid.NewGuid());
            Task.Delay(5000).Wait();
            client.Writer.SendMessage(new RequestMessage(1, null, server.DeviceId, Encoding.ASCII.GetBytes("abbcdefg")));
            var response = (ResponseMessage)client.Reader.ReceiveMessage();
            Assert.Equal("unknown command", Encoding.ASCII.GetString(response.RequestData));
        }
    }
}
