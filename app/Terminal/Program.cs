using Protocol;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "localhost";
            int port = 21008;
            if (args.Length == 2)
            {
                host = args[1];
                port = Convert.ToInt32(args[2]);
            }

            var terminal = new Terminal();
            terminal.Run(host, port);
        }
    }
}
