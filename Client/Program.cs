using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SntpLib;

namespace Client
{
    public static class Program
    {
        //private static readonly DnsEndPoint ServerAddress = new DnsEndPoint("time.windows.com", 123);
        private static readonly IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123);

        public static void Main()
        {
            Thread.Sleep(500);

            using (var client = new Socket(SocketType.Dgram, ProtocolType.Udp))
                try
                {
                    Console.WriteLine(Request.UtcRequest(client, serverAddress));
                }
                catch (IncorrectPackageFormatException exception)
                {
                    Console.WriteLine(exception.Message);
                }
        }
    }
}