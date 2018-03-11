using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;


namespace Client
{
    class Request
    {
        private static UInt32 FromBytesToInt(IEnumerable<byte> bytes)
        {
            var byteStrings = bytes.Select(_byte => Convert.ToString(_byte, 2).PadLeft(8, '0'));
            var joinedString = String.Join("", byteStrings);
            return Convert.ToUInt32(joinedString, 2);
        }

        private static double TotalSeconds(byte[] timeBuffer)
        {
            var intPart = FromBytesToInt(timeBuffer.Take(4));
            var floatPart = FromBytesToInt(timeBuffer.Skip(4));
            var floatPartLength = floatPart.ToString().Length;
            return intPart + floatPart * Math.Pow(10, -floatPartLength);
        }

        private static DateTime ResultUtcComputing(SntpLib.SNTPMessage sntpMessage, DateTime destinationTimestamp)
        {
            var originateSeconds = TotalSeconds(sntpMessage.OriginTimestamp.ByteArray);
            var receiveSeconds = TotalSeconds(sntpMessage.ReceiveTimestamp.ByteArray);
            var transmitSeconds = TotalSeconds(sntpMessage.TransmitTimestamp.ByteArray);
            var destinationSeconds = (destinationTimestamp - SntpLib.SNTPMessage.Era0).TotalSeconds;
            var offset = 0.5 * (receiveSeconds - originateSeconds + transmitSeconds - destinationSeconds);
            return destinationTimestamp.AddSeconds(offset);
        }

        public static DateTime UtcRequest(Socket client, EndPoint serverAddress)
        {
            client.Connect(serverAddress);
            var firstMessage = new SntpLib.SNTPMessage(SntpLib.ModeType.ClientMode);
            client.Send(firstMessage.ResultMessage);
            var serverResponse = new byte[SntpLib.SNTPMessage.PackageBytesCount];
            client.Receive(serverResponse);
            var destinationTimestamp = DateTime.UtcNow;
            var secondMessage = new SntpLib.SNTPMessage(serverResponse);
            secondMessage.OriginTimestamp.ByteArray = firstMessage.OriginTimestamp.ByteArray;
            return ResultUtcComputing(secondMessage, destinationTimestamp);
        }
    }
}