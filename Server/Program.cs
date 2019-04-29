using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using CommandLine;
using SntpLib;

namespace Server
{
    static class Program
    {
        private const int NtpPortNumber = 123;

        private static void Main(string[] args)
        {
            var secondsOffset = 0;
            var receiveTimeout = 0;

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(options =>
                  {
                      secondsOffset = options.SecondsOffset;
                      receiveTimeout = options.ReceiveTimeout;
                  })
                  .WithNotParsed(errors => Environment.Exit(0));

            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                try
                {
                    server.Bind(new IPEndPoint(IPAddress.Any, NtpPortNumber));
                }
                catch (Exception exception) when (exception is SocketException || exception is SecurityException)
                {
                    Console.WriteLine($"Problem with access to {NtpPortNumber} port.");

                    Environment.Exit(0);
                }

                try
                {
                    while (true)
                    {
                        EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                        Replay.UtcReplay(server, remoteAddress, secondsOffset, receiveTimeout);
                    }
                }
                catch (IncorrectPackageFormatException exception)
                {
                    Console.WriteLine(exception.Message);
                }
                catch (SocketException exception) when (exception.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Time is out.");
                }
            }
        }
    }
}