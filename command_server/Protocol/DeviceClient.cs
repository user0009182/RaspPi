using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class DeviceClient
    {
        TcpClient tcpClient;
        SslStream sslStream;
        NetworkStream networkStream;
        DeviceProtocolReader reader;
        DeviceProtocolWriter writer;
        EventTracer trace;
        public Guid RemoteDeviceId { get; set; }

        public DeviceProtocolReader Reader
        {
            get
            {
                return reader;
            }
        }

        public DeviceProtocolWriter Writer
        {
            get
            {
                return writer;
            }
        }

        public EndPoint RemoteEndpoint { get; private set; }

        /// <summary>
        /// Name of the remote device. This is initially provided by the device itself. It isn't guaranteed to be unique.
        /// </summary>
        public string RemoteName { get; private set; } = "unnamed";

        /// <summary>
        /// Name the local device
        /// </summary>
        public string LocalName { get; private set; } = "unnamed";

        public DeviceCapabilities Capabilities { get; internal set; }
        public bool IsConnected
        {
            get
            {
                return tcpClient.Connected;
            }
        }

        public IdleTimeoutPolicy IdleTimeoutPolicy { get; private set; } = new IdleTimeoutPolicy(30, false);

        public void SetIdleTimeoutPolicy(int idleTimeoutInterval, bool sendKeepaliveResponses)
        {
            IdleTimeoutPolicy = new IdleTimeoutPolicy(idleTimeoutInterval, sendKeepaliveResponses);
        }

        public BaseMessage TryReadMessage(int timeout)
        {
            if (timeout < 1)
                timeout = 1;
            SetRecvTimeout(timeout);
            try
            {
                return Reader.ReceiveMessage();
            }
            catch (Exception e)
            {
                if (DeviceProtocolException.IsSocketTimeoutException(e))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            
        }

        public DeviceClient(string localName, ITraceSink traceSink)
        {
            this.LocalName = localName;
            this.trace = new EventTracer(traceSink);
        }

        internal void AssignRemoteName(string name)
        {
            RemoteName = name;
        }

        /// <summary>
        /// Create new device client to wrap an existing connection
        /// such as from an accepted socket
        /// </summary>
        public void WrapTcpClient(TcpClient client, TlsInfo tlsInfo)
        {
            this.tcpClient = client;
            RemoteEndpoint = client.Client.RemoteEndPoint;
            networkStream = client.GetStream();
            Stream stream = null;
            if (tlsInfo.UseTls)
            {
                trace.Flow(TraceEventId.TlsAuthenticatingAsServer, tlsInfo.CertificatePath, tlsInfo.KeyPath);
                sslStream = new SslStream(networkStream, false);
                var serverCertificate = X509Certificate2.CreateFromPemFile(tlsInfo.CertificatePath, tlsInfo.KeyPath);
                var serverCertificate2 = new X509Certificate2(serverCertificate.Export(X509ContentType.Pkcs12));
                sslStream.AuthenticateAsServer(serverCertificate2, clientCertificateRequired: true, checkCertificateRevocation: false);
                trace.Flow(TraceEventId.TlsAuthenticationSuccess);
                stream = sslStream;
            }
            else
            {
                stream = networkStream;
            }
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;
            writer = new DeviceProtocolWriter(new BinaryWriter(stream));
            reader = new DeviceProtocolReader(new BinaryReader(stream));
        }

        public string SendStringRequest(string request)
        {
            return SendStringRequest(null, request);
        }

        public string SendStringRequest(string target, string request)
        {
            Guid targetGuid = Guid.Empty;
            if (target == null)
                targetGuid = RemoteDeviceId;
            Writer.SendMessage(new RequestMessage(1, target, targetGuid, System.Text.Encoding.ASCII.GetBytes(request)));
            var response = Reader.ReceiveMessage() as ResponseMessage;
            if (response == null)
                return null;
            var responseText = System.Text.Encoding.ASCII.GetString(response.RequestData);
            return responseText;
        }

        public void Close()
        {
            if (sslStream != null)
            {
                sslStream.Close();
            }
            if (tcpClient != null)
            {
                tcpClient.Client.Close();
                tcpClient.Close();
            }
        }

        /// <summary>
        /// Create a new connection as a client
        /// </summary>
        public bool Connect(string hostname, int port, TlsInfo tlsInfo, Guid ownDeviceId) // bool tls, string clientCertificateFilePath, string clientKeyPath)
        {
            if (tlsInfo == null)
                tlsInfo = new TlsInfo(false, "", "");
            trace.Flow(TraceEventId.ConnectingAsClient, hostname, Convert.ToString(port), tlsInfo.UseTls.ToString());
            tcpClient = new TcpClient(hostname, port);
            trace.Flow(TraceEventId.ConnectedAsClient, hostname, Convert.ToString(port), tlsInfo.UseTls.ToString());
            RemoteEndpoint = tcpClient.Client.RemoteEndPoint;
            networkStream = tcpClient.GetStream();
            Stream stream;
            if (tlsInfo.UseTls)
            {
                trace.Detail(TraceEventId.ClientCertificatePath, tlsInfo.CertificatePath, tlsInfo.KeyPath);
                var clientCertificate = X509Certificate2.CreateFromPemFile(tlsInfo.CertificatePath, tlsInfo.KeyPath);
                clientCertificate = new X509Certificate2(clientCertificate.Export(X509ContentType.Pkcs12));
                var clientCertificateCollection = new X509CertificateCollection(new X509Certificate[] { clientCertificate });
                sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                trace.Detail(TraceEventId.TlsAuthenticatingAsClient);
                sslStream.AuthenticateAsClient(targetHost: "test.com", clientCertificateCollection, false);
                trace.Detail(TraceEventId.TlsAuthenticationSuccess);
                stream = sslStream;
            }
            else
            {
                stream = networkStream;
            }
            writer = new DeviceProtocolWriter(new BinaryWriter(stream));
            reader = new DeviceProtocolReader(new BinaryReader(stream));
            //TODO default values
            reader.SetTimeout(10000);
            writer.SetTimeout(10000);
            var handshake = new DeviceHandshake(trace);
            trace.Detail(TraceEventId.HandshakeAsClientBegin);
            bool success = handshake.DoHandshakeAsClient(this, ownDeviceId);
            if (!success)
            {
                trace.Failure(TraceEventId.HandshakeAsClientFailed);
                return false;
            }
            trace.Detail(TraceEventId.HandshakeAsClientSuccess);
            return true;
        }

        public void SetSendTimeout(int timeout)
        {
            writer.SetTimeout(timeout);
        }

        public void SetRecvTimeout(int timeout)
        {
            reader.SetTimeout(timeout);
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                trace.Detail(TraceEventId.ServerCertificateValidationSuccess);
                return true;
            }
            trace.Failure(TraceEventId.ServerCertificateValidationFailed);
            return false;
        }
    }

    public enum DeviceCapabilities
    {
        Simple = 0,
        Router = 1
    }

    public struct IdleTimeoutPolicy
    {
        public int Interval;
        public bool SendKeepaliveResponses;

        public IdleTimeoutPolicy(int interval, bool sendKeepaliveResponses)
        {
            Interval = interval;
            SendKeepaliveResponses = sendKeepaliveResponses;
        }
    }
}
