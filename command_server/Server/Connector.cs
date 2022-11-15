using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Connector
    {
        DeviceClient client;
        ConnectorHandler handler;
        public Connector(ConnectorHandler handler)
        {
            this.handler = handler;
        }

        public void Connect(string hostname, int port, ConnectorSecurityInfo securityInfo)
        {
            client = new DeviceClient(null);
            client.Connect(hostname, port, null); // false, securityInfo.ClientCertificatePath, securityInfo.ClientPrivateKeyPath);
            Task.Run(() => ThreadProc(hostname, port, securityInfo));
        }

        void ThreadProc(string hostname, int port, ConnectorSecurityInfo securityInfo)
        {
            Retryer retry = new Retryer();
            retry.Run(() =>
               {
                   try
                   {
                       //client.ConnectTo(hostname, port, securityInfo.ClientCertificatePath, securityInfo.ClientPrivateKeyPath);
                       handler.OnConnected(this);
                       return Retryer.RetryDecision.DontRetry;
                   }
                   catch (Exception)
                   {
                       return Retryer.RetryDecision.Retry;
                   }
               });
        }

        internal string ReceiveCommand()
        {
            //var commandLength = client.Reader.ReadUInt16();
            //if (commandLength > MAX_RECV_PACKET_SIZE)
            //{
            //    Debug.WriteLine($"Received length {dataLength} exceeds MAX_RECV_PACKET_SIZE {MAX_RECV_PACKET_SIZE}");
            //    return null;
            //}
            //var data = client.Reader.ReadBytes(commandLength);
            //var command = Encoding.ASCII.GetString(data);
            return ""; // command;


            //Debug.WriteLine("received " + Encoding.ASCII.GetString(data));
            //client.Recv(commandLenth);
        }
    }

    class Retryer
    {
        public void Run(Func<RetryDecision> action)
        {
            RetryDecision retry = RetryDecision.Retry;
            while (retry == RetryDecision.Retry)
            {
                retry = action();
                Task.Delay(10000).Wait();
            }
        }

        public enum RetryDecision
        { 
            DontRetry,
            Retry
        }
    }

    class ConnectorHandler
    {
        DeviceServer server;
        public ConnectorHandler(DeviceServer server)
        {
            this.server = server;
        }

        public void OnConnected(Connector connector)
        {
            var command = connector.ReceiveCommand();
            //issue command to server and await response
            Program.ProcessCommand(server, command);
            //wait for command
            //connector.recv
            //expect ping every minute


        }
    }

    class ConnectorSecurityInfo
    {
        public ConnectorSecurityInfo(string caCertificatePath, string clientCertificatePath, string clientPrivateKeyPath)
        {
            CaCertificatePath = caCertificatePath;
            ClientCertificatePath = clientCertificatePath;
            ClientPrivateKeyPath = clientPrivateKeyPath;
        }

        public string CaCertificatePath { get; }
        public string ClientCertificatePath { get; }
        public string ClientPrivateKeyPath { get; }
    }
}
