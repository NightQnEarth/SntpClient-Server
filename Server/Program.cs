using System;
using System.Net.Sockets;
using System.Net;
using CommandLine;

namespace Server
{
    class Program
    {
        public const int NtpPortNumber = 123;

        static void Main(string[] args)
        {
            var secondsOffset = 0;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => secondsOffset = opts.SecondsOffset)
                .WithNotParsed((errors) => Environment.Exit(0));

            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, NtpPortNumber));

                EndPoint client = new IPEndPoint(IPAddress.Any, 0);
                Replay.UtcReplay(server, client, secondsOffset);
            }
            catch (SntpLib.IncorrectPackageFormatException exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                server.Close();
            }
        }
    }
}