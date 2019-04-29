using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using SntpLib;

namespace Client
{
    static class Request
    {
        private static uint FromBytesToIntConvert(IEnumerable<byte> bytes)
        {
            var byteStrings = bytes.Select(_byte => Convert.ToString(_byte, 2).PadLeft(8, '0'));
            var joinedString = string.Concat(byteStrings);

            return Convert.ToUInt32(joinedString, 2);
        }

        private static double GetTotalSeconds(byte[] timeBuffer)
        {
            var intPart = FromBytesToIntConvert(timeBuffer.Take(4));
            var floatPart = FromBytesToIntConvert(timeBuffer.Skip(4));
            var floatPartLength = floatPart.ToString().Length;

            return intPart + floatPart * Math.Pow(10, -floatPartLength);
        }

        private static DateTime ComputeUtcResult(SNTPMessage sntpMessage, DateTime destinationTimestamp)
        {
            var originateSeconds = GetTotalSeconds(sntpMessage.OriginTimestamp.ByteArray);
            var receiveSeconds = GetTotalSeconds(sntpMessage.ReceiveTimestamp.ByteArray);
            var transmitSeconds = GetTotalSeconds(sntpMessage.TransmitTimestamp.ByteArray);

            var destinationSeconds = (destinationTimestamp - SNTPMessage.Era0).TotalSeconds;
            var offset = 0.5 * (receiveSeconds - originateSeconds + transmitSeconds - destinationSeconds);

            return destinationTimestamp.AddSeconds(offset);
        }

        public static DateTime UtcRequest(Socket client, EndPoint serverAddress)
        {
            client.Connect(serverAddress);

            var firstMessage = new SNTPMessage(ModeType.ClientMode);
            client.Send(firstMessage.ResultMessage);

            var serverResponse = new byte[SNTPMessage.PackageBytesCount];
            client.Receive(serverResponse);

            var destinationTimestamp = DateTime.UtcNow;

            var secondMessage = new SNTPMessage(serverResponse);
            secondMessage.OriginTimestamp.ByteArray = firstMessage.OriginTimestamp.ByteArray;

            return ComputeUtcResult(secondMessage, destinationTimestamp);
        }
    }
}