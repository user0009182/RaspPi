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

namespace Server
{
    public class DeviceClient
    {
        TcpClient client;
        SslStream sslStream;
        NetworkStream networkStream;
        DeviceProtocolReader reader;
        DeviceProtocolWriter writer;
        ILogger logger;
        public DeviceClient(ILogger logger)
        {
            this.logger = logger;
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
            stream.ReadTimeout = 60000;
            stream.WriteTimeout = 60000;
            writer = new DeviceProtocolWriter(new BinaryWriter(stream));
            reader = new DeviceProtocolReader(new BinaryReader(stream));
        }

        /// <summary>
        /// Create a new connection as a client
        /// </summary>
        public void Connect(string hostname, int port, TlsInfo tlsInfo) // bool tls, string clientCertificateFilePath, string clientKeyPath)
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
            DoHandshakeAsClient();
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

        //const int MAX_RECV_PACKET_SIZE = 18192; //4096;
        //public byte[] ReceiveDataWithTimeout(int timeoutMs)
        //{
        //    networkStream.ReadTimeout = timeoutMs;
        //    try
        //    {
        //        ushort dataLength = reader.ReadUInt16();
        //        if (dataLength > MAX_RECV_PACKET_SIZE)
        //        {
        //            //Debug.WriteLine($"Received length {dataLength} exceeds MAX_RECV_PACKET_SIZE {MAX_RECV_PACKET_SIZE}");
        //            return null;
        //        }
        //        var data = reader.ReadBytes(dataLength);
        //        //Debug.WriteLine("received " + Encoding.ASCII.GetString(data));
        //        return data;
        //    }
        //    catch (IOException)
        //    {
        //        return null;
        //    }
        //}

        //public bool SendDataWithTimeout(byte[] data, int timeoutMs)
        //{
        //    networkStream.WriteTimeout = timeoutMs;
        //    try
        //    {
        //        writer.Write((ushort)(data.Length));
        //        writer.Write(data);
        //        //Debug.WriteLine("sent " + Encoding.ASCII.GetString(data));
        //        return true;
        //    }
        //    catch (IOException)
        //    {
        //        return false;
        //    }
        //}

        public bool DoHandshakeAsServer()
        {
            logger.Log($"begin device protocol handshake as server");
            //TODO reader.settimeout
            var data = reader.ReadData16();
            if (data == null)
                return false;
            if (Encoding.ASCII.GetString(data) != "device")
                return false;
            //TODO timeout catch IOException on receive?
            writer.WriteData16(Encoding.ASCII.GetBytes("server"));
            data = reader.ReadData16();
            if (data == null)
                return false;
            if (Encoding.ASCII.GetString(data) != "ok")
                return false;
            //var sent = SendDataWithTimeout(Encoding.ASCII.GetBytes("server"), 10000);
            //if (!sent)
            //    return false;
            //data = ReceiveDataWithTimeout(10000);

            logger.Log($"device protocol handshake complete OK");
            return true;
        }

        public bool DoHandshakeAsClient()
        {
            logger.Log($"begin device protocol handshake as client");
            writer.WriteData16(Encoding.ASCII.GetBytes("device"));
            var data = reader.ReadData16();
            if (Encoding.ASCII.GetString(data) != "server")
                return false;
            writer.WriteData16(Encoding.ASCII.GetBytes("ok"));
            logger.Log($"handshake complete OK");
            return true;
        }
    }
}
