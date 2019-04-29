using System;
using System.Net;
using System.Net.Sockets;
using SntpLib;

namespace Server
{
    static class Replay
    {
        private const int SendTimeout = 1000 * 5;

        public static void UtcReplay(Socket server, EndPoint remoteAddress, int secondsOffset, int receiveTimeout)
        {
            server.ReceiveTimeout = receiveTimeout;
            server.SendTimeout = SendTimeout;

            var clientRequest = new byte[SNTPMessage.PackageBytesCount];
            server.ReceiveFrom(clientRequest, ref remoteAddress);

            var destinationTimestamp = DateTime.UtcNow;

            var serverReplay = new SNTPMessage(clientRequest)
            {
                Mode = ModeType.ServerMode
            };

            serverReplay.ReceiveTimestamp.ByteArray = SNTPMessage.ConvertDateTimeToBytes(destinationTimestamp);
            serverReplay.TransmitTimestamp.ByteArray = SNTPMessage.ConvertDateTimeToBytes(
                DateTime.UtcNow.AddSeconds(secondsOffset));
            server.SendTo(serverReplay.ResultMessage, remoteAddress);
        }
    }
}