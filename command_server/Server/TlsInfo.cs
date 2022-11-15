using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TlsInfo
    {
        public TlsInfo(bool enableTls, string certificatePath, string keyPath)
        {
            UseTls = enableTls;
            CertificatePath = certificatePath;
            KeyPath = keyPath;
        }

        public bool UseTls { get; set; }
        public string CertificatePath { get; set; }
        public string KeyPath { get; set; }
    }
}
