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
        TcpClient client;
        SslStream sslStream;
        NetworkStream networkStream;
        DeviceProtocolReader reader;
        DeviceProtocolWriter writer;
        ILogger logger;
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
        /// Name of the device. This is initially provided by the device itself. It isn't guaranteed to be unique.
        /// </summary>
        public string Name { get; private set; } = "unnamed";
        public DeviceCapabilities Capabilities { get; internal set; }

        public DeviceClient(ILogger logger)
        {
            this.logger = logger;
        }

        public void AssignName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create new device client to wrap an existing connection
        /// such as from an accepted socket
        /// </summary>
        public void WrapTcpClient(TcpClient client, TlsInfo tlsInfo)
        {
            this.client = client;
            RemoteEndpoint = client.Client.RemoteEndPoint;
            networkStream = client.GetStream();
            Stream stream = null;
            if (tlsInfo.UseTls)
            {
                logger.Log($"TLS authenticating as server using certificate {tlsInfo.CertificatePath}, key {tlsInfo.KeyPath}");
                sslStream = new SslStream(networkStream, false);
                var serverCertificate = X509Certificate2.CreateFromPemFile(tlsInfo.CertificatePath, tlsInfo.KeyPath);
                var serverCertificate2 = new X509Certificate2(serverCertificate.Export(X509ContentType.Pkcs12));
                sslStream.AuthenticateAsServer(serverCertificate2, clientCertificateRequired: true, checkCertificateRevocation: false);
                logger.Log($"TLS authentication OK");
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

        public void Close()
        {
            if (sslStream != null)
            {
                sslStream.Close();
            }
            if (client != null)
            {
                client.Close();
            }
        }

        /// <summary>
        /// Create a new connection as a client
        /// </summary>
        public void Connect(string hostname, int port, TlsInfo tlsInfo, Guid ownDeviceId) // bool tls, string clientCertificateFilePath, string clientKeyPath)
        {
            if (tlsInfo == null)
                tlsInfo = new TlsInfo(false, "", "");
            logger.Log($"connecting to {hostname}:{port} tls:{tlsInfo.UseTls}");
            client = new TcpClient(hostname, port);
            logger.Log($"connected to {hostname}:{port} tls:{tlsInfo.UseTls}");
            RemoteEndpoint = client.Client.RemoteEndPoint;
            networkStream = client.GetStream();
            Stream stream;
            if (tlsInfo.UseTls)
            {
                logger.Log($"client certificate path='{tlsInfo.CertificatePath}', key path='{tlsInfo.KeyPath}'");
                var clientCertificate = X509Certificate2.CreateFromPemFile(tlsInfo.CertificatePath, tlsInfo.KeyPath);
                clientCertificate = new X509Certificate2(clientCertificate.Export(X509ContentType.Pkcs12));
                var clientCertificateCollection = new X509CertificateCollection(new X509Certificate[] { clientCertificate });
                sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                logger.Log($"TLS authenticating as client");
                sslStream.AuthenticateAsClient(targetHost: "test.com", clientCertificateCollection, false);
                logger.Log($"TLS authenticated OK");
                stream = sslStream;
            }
            else
            {
                stream = networkStream;
            }
            writer = new DeviceProtocolWriter(new BinaryWriter(stream));
            reader = new DeviceProtocolReader(new BinaryReader(stream));
            reader.SetTimeout(30000);
            writer.SetTimeout(30000);
            var handshake = new DeviceHandshake(logger);
            logger.Log($"begin handshake with server");
            bool success = handshake.DoHandshakeAsClient(this, ownDeviceId);
            if (!success)
            {
                logger.Log($"handshake with server failed");
                return;
            }
            logger.Log($"handshake with server complete");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                logger.Log($"server certificate validated OK");
                return true;
            }
            logger.Log($"failed to validate server certificate: {sslPolicyErrors}");
            return false;
        }
    }

    public enum DeviceCapabilities
    {
        Simple = 0,
        Router = 1
    }
}
