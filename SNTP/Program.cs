using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace Client
{
    public class Program
    {
        //private static readonly DnsEndPoint ServerAddress = new DnsEndPoint("time.windows.com", 123);
        private static readonly IPEndPoint ServerAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123);

        public static void Main(string[] args)
        {
            Thread.Sleep(500);
            var client = new Socket(SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine(Request.UtcRequest(client, ServerAddress));
            client.Close();
        }
    }
}