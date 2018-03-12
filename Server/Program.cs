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
            var receiveTimeout = 0;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => 
                {
                    secondsOffset = opts.SecondsOffset;
                    receiveTimeout = opts.ReceiveTimeout;
                })
                .WithNotParsed((errors) => Environment.Exit(0));

            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(new IPEndPoint(IPAddress.Any, NtpPortNumber));

            try
            {
                while (true)
                {
                    EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                    Replay.UtcReplay(server, remoteAddress, secondsOffset, receiveTimeout);
                }
            }
            catch (SntpLib.IncorrectPackageFormatException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (SocketException exception)
            {
                if (exception.SocketErrorCode == SocketError.TimedOut)
                    Console.WriteLine("Time is out.");
                else
                    throw;
            }
            finally
            {
                server.Close();
            }
        }
    }
}