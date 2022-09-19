using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ClientHandler
    {
        private TcpClient client;
        NetworkStream networkStream;
        Server server;
        public EndPoint RemoteEndpoint { get; private set; }

        public ClientHandler(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
        }

        public void Run()
        {
            networkStream = client.GetStream();
            RemoteEndpoint = client.Client.RemoteEndPoint;
            bool isConnected = ConnectHandshake();
            if (isConnected)
            {
                server.RegisterConnectedClient(this);
                ProcessLoop();
                server.DeregisterClient(this);
            }
            Disconnect();

        }

        bool ConnectHandshake()
        {
            var data = ReceiveDataWithTimeout(10000);
            if (data == null)
                return false;
            //TODO decrypt
            if (Encoding.ASCII.GetString(data) != "device")
                return false;
            var sent = SendDataWithTimeout(Encoding.ASCII.GetBytes("server"), 10000);
            if (!sent)
                return false;
            data = ReceiveDataWithTimeout(10000);
            if (data == null)
                return false;
            if (Encoding.ASCII.GetString(data) != "ok")
                return false;
            return true;
        }

        void Disconnect()
        {
            client.Close();
            Debug.WriteLine("client disconnected");
        }

        public string EnqueueCommand(string command)
        {
            var request = new SendRequest() { Command = command };
            sendRequestQueue.Enqueue(request);
            Debug.WriteLine("request enqueued");
            commandResetEvent.Set();
            request.resetEvent.WaitOne();
            Debug.WriteLine("request complete");
            return request.Response;
        }
        ConcurrentQueue<SendRequest> sendRequestQueue = new ConcurrentQueue<SendRequest>();
        AutoResetEvent commandResetEvent = new AutoResetEvent(false);

        void ProcessLoop()
        {
            while (true)
            {
                SendRequest request;
                if (sendRequestQueue.TryDequeue(out request))
                {
                    Debug.WriteLine("request dequeued");
                    var response = SendCommand(request.Command);
                    request.SetResponse(response);
                    continue;
                }

                bool signal = commandResetEvent.WaitOne(5000);
                if (signal)
                {
                    continue;
                }

                bool pongReceived = SendPing();
                if (!pongReceived)
                {
                    break;
                }
            }
        }

        bool SendPing()
        {
            SendDataWithTimeout(Encoding.ASCII.GetBytes("ping"), 5000);
            var data = ReceiveDataWithTimeout(5000);
            if (data == null)
            {
                return false;
            }
            return true;
        }

        string SendCommand(string command)
        {
            SendDataWithTimeout(Encoding.ASCII.GetBytes(command), 5000);
            var data = ReceiveDataWithTimeout(5000);
            if (data == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(data);
        }

        const int MAX_RECV_PACKET_SIZE = 1024;
        byte[] ReceiveDataWithTimeout(int timeoutMs)
        {
            var reader = new BinaryReader(networkStream);
            networkStream.ReadTimeout = timeoutMs;
            try
            {
                ushort dataLength = reader.ReadUInt16();
                if (dataLength > 1024)
                {
                    Debug.WriteLine($"Received length {dataLength} exceeds MAX_RECV_PACKET_SIZE {MAX_RECV_PACKET_SIZE}");
                    return null;
                }
                var data = reader.ReadBytes(dataLength);
                Debug.WriteLine("received " + Encoding.ASCII.GetString(data));
                return data;
            }
            catch (IOException)
            {
                return null;
            }
        }

        bool SendDataWithTimeout(byte[] data, int timeoutMs)
        {
            var writer = new BinaryWriter(networkStream);
            networkStream.WriteTimeout = timeoutMs;
            try
            {
                writer.Write((ushort)(data.Length));
                writer.Write(data);
                Debug.WriteLine("sent " + Encoding.ASCII.GetString(data));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }

    class SendRequest
    {
        public AutoResetEvent resetEvent = new AutoResetEvent(false);
        public string Command;
        public string Response;

        internal void SetResponse(string response)
        {
            Response = response;
            resetEvent.Set();
        }
    }
}

