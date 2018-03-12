using System;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class Replay
    {
        public static void UtcReplay(Socket server, EndPoint remoteAddress, int secondsOffset, int receiveTimeout)
        {
            server.ReceiveTimeout = receiveTimeout;
            server.SendTimeout = 1000 * 5;

            var clientRequest = new byte[SntpLib.SNTPMessage.PackageBytesCount];
            var bytesReceived = server.ReceiveFrom(clientRequest, ref remoteAddress);

            var destinationTimestamp = DateTime.UtcNow;

            var serverReplay = new SntpLib.SNTPMessage(clientRequest)
            {
                Mode = SntpLib.ModeType.ServerMode
            };
            serverReplay.ReceiveTimestamp.ByteArray =
                SntpLib.SNTPMessage.ConvertDateTimeToBytes(destinationTimestamp);
            serverReplay.TransmitTimestamp.ByteArray =
                SntpLib.SNTPMessage.ConvertDateTimeToBytes(DateTime.UtcNow.AddSeconds(secondsOffset));

            server.SendTo(serverReplay.ResultMessage, remoteAddress);
        }
    }
}