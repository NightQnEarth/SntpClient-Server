using System;
using System.Net.Sockets;
using System.Net;
using CommandLine;
using System.Security;

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

            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, NtpPortNumber));
            }
            catch (Exception ex) when (ex is SocketException || ex is SecurityException)
            {
                Console.WriteLine("Problem with access to 123 port.");
                server.Close();
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
            catch (SntpLib.IncorrectPackageFormatException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (SocketException exception) when 
                (exception.SocketErrorCode == SocketError.TimedOut)
            {
                Console.WriteLine("Time is out.");
            }
            finally
            {
                server.Close();
            }
        }
    }
}