using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Background thread maintaining outgoing connections
    /// Reconnects if the connection is lost
    /// </summary>
    public class OutgoingConnectionProcessor
    {
        //List of outgoing connections that should be made
        List<OutgoingConnectionInfo> outgoingConnections = new List<OutgoingConnectionInfo>();
        CancellationTokenSource threadCancellationSource = new CancellationTokenSource();
        CancellationTokenSource retryCancellationSource = new CancellationTokenSource();

        Server server;
        ILogger logger;
        public OutgoingConnectionProcessor(Server server, ILogger logger)
        {
            this.server = server;
            this.logger = logger;
        }

        public void RegisterOutgoingConnection(string host, int port, TlsInfo tlsInfo)
        {
            lock(outgoingConnections)
            {
                outgoingConnections.Add(new OutgoingConnectionInfo(host, port, tlsInfo));
                retryCancellationSource.Cancel();
            }
        }

        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        public void Stop()
        {
            threadCancellationSource.Cancel();
            retryCancellationSource.Cancel();
        }

        void ThreadProc()
        {
            while (!threadCancellationSource.IsCancellationRequested)
            {
                var connectionInfo = GetNextConnectionToRetry();
                if (connectionInfo == null)
                {
                    DelayForRetry(9999);
                    continue;
                }
                var secondsToRetry = connectionInfo.SecondsUntilRetry();
                if (secondsToRetry > 1)
                {
                    DelayForRetry(secondsToRetry);
                    continue;
                }

                AttemptConnection(connectionInfo);
            }
        }

        void DelayForRetry(int seconds)
        {
            try
            {
                Task.Delay(seconds * 1000).Wait(retryCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                retryCancellationSource = new CancellationTokenSource();
            }
        }

        void AttemptConnection(OutgoingConnectionInfo connection)
        {
            try
            {
                var client = new DeviceClient(server.Name, logger);
                var success = client.Connect(connection.Host, connection.Port, connection.TlsInfo, server.DeviceId);
                if (!success)
                {
                    connection.NextRetry = DateTime.Now.AddSeconds(30);
                    return;
                }
                var handler = server.CreateDeviceClientHandler(client);
                connection.SetHandler(handler);
            }
            catch (Exception exception)
            {
                connection.NextRetry = DateTime.Now.AddSeconds(30);
            }
        }

        OutgoingConnectionInfo GetNextConnectionToRetry()
        {
            lock (outgoingConnections)
            {
                var retryConnection = outgoingConnections.Where(c => !c.IsConnected).OrderBy(c => c.SecondsUntilRetry()).FirstOrDefault();
                return retryConnection;
            }
        }

        internal void OnDisconnect(DeviceClientHandler handler)
        {
            lock (outgoingConnections)
            {
                var connection = outgoingConnections.FirstOrDefault(c => c.Handler == handler);
                if (connection != null)
                {
                    connection.SetHandler(null);
                    retryCancellationSource.Cancel();
                }
            }
        }
    }

    /// <summary>
    /// Represents the state of an outgoing connection
    /// </summary>
    public class OutgoingConnectionInfo
    {
        public OutgoingConnectionInfo(string host, int port, TlsInfo tlsInfo)
        {
            Host = host;
            Port = port;
            TlsInfo = tlsInfo;
            NextRetry = DateTime.Now;
        }

        public string Host { get; }
        public int Port { get; }
        public TlsInfo TlsInfo { get; }
        public bool IsConnected
        {
            get
            {
                return Handler != null;
            }
        }
        public DateTime NextRetry { get; set; }
        public DeviceClientHandler Handler { get; private set; }
        public int SecondsUntilRetry()
        {
            return (int)NextRetry.Subtract(DateTime.Now).TotalSeconds;
        }

        internal void SetHandler(DeviceClientHandler handler)
        {
            this.Handler = handler;
        }
    }
}
