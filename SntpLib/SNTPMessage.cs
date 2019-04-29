using System;
using System.Globalization;
using System.Linq;

namespace SntpLib
{
    public class SNTPMessage
    {
        public static readonly DateTime Era0 = new DateTime(1900, 1, 1);
        public const int PackageBytesCount = 48;

        public const string LeapIndicator = "00";
        public const string VersionNumber = "100";
        public ModeType Mode { get; set; }
        public readonly PackageField Stratum = new PackageField(1, new byte[1]);
        public readonly PackageField Poll = new PackageField(2, new byte[1]);
        public readonly PackageField Precision = new PackageField(3, new byte[1]);
        public readonly PackageField RootDelay = new PackageField(4, new byte[4]);
        public readonly PackageField RootDispersion = new PackageField(8, new byte[4]);
        public readonly PackageField ReferenceId = new PackageField(12, new byte[4]);
        public readonly PackageField ReferenceTimestamp = new PackageField(16, new byte[8]);
        public readonly PackageField OriginTimestamp = new PackageField(24, new byte[8]);
        public readonly PackageField ReceiveTimestamp = new PackageField(32, new byte[8]);
        public readonly PackageField TransmitTimestamp = new PackageField(40, new byte[8]);

        private readonly PackageField[] fields;

        private void UpdateFirstByte()
        {
            string firstByte = string.Join("", LeapIndicator, VersionNumber, Mode.ToString());
            resultMessage[0] = Convert.ToByte(firstByte, 2);
        }

        private byte[] resultMessage;

        public byte[] ResultMessage
        {
            get
            {
                UpdateFirstByte();
                foreach (var field in fields)
                    field.ByteArray.CopyTo(resultMessage, field.PackageStartByte);
                return resultMessage;
            }
            private set => resultMessage = value;
        }

        private SNTPMessage()
        {
            ResultMessage = new byte[PackageBytesCount];
            fields = new[]
            {
                Stratum, Poll, Precision, RootDelay,
                RootDispersion, ReferenceId, ReferenceTimestamp,
                OriginTimestamp, ReceiveTimestamp, TransmitTimestamp
            };
        }

        public SNTPMessage(ModeType modeType) : this()
        {
            Mode = modeType;
            UpdateFirstByte();
            if (Mode == ModeType.ClientMode)
                OriginTimestamp.ByteArray = ConvertDateTimeToBytes(DateTime.UtcNow);
        }

        public SNTPMessage(byte[] buffer) : this()
        {
            try
            {
                int modeStartBit = LeapIndicator.Length + VersionNumber.Length;
                string firstByte = Convert.ToString(buffer[0], 2).PadLeft(8, '0');
                Mode = (ModeType)firstByte.Substring(modeStartBit);
                UpdateFirstByte();

                foreach (var field in fields)
                    buffer.Skip(field.PackageStartByte)
                          .Take(field.ByteArray.Length)
                          .ToArray()
                          .CopyTo(field.ByteArray, 0);
            }
            catch (Exception exception) when (exception is InvalidCastException ||
                                              exception is ArrayTypeMismatchException ||
                                              exception is ArgumentOutOfRangeException ||
                                              exception is ArgumentException)
            {
                throw new IncorrectPackageFormatException(exception.Message, exception.InnerException);
            }
        }

        public static byte[] ConvertDateTimeToBytes(DateTime dateTime)
        {
            double destinationSeconds = (dateTime - Era0).TotalSeconds;

            var intPart = Convert.ToUInt32(destinationSeconds.ToString(CultureInfo.InvariantCulture).Split('.')[0]);
            var floatPart = (long)((destinationSeconds - intPart) *
                                   (long)Math.Pow(10, (destinationSeconds - intPart)
                                                      .ToString(CultureInfo.InvariantCulture).Length - 2));
            var intStr = Convert.ToString(intPart, 2).PadLeft(32, '0');
            var floatStr = Convert.ToString(floatPart, 2).PadLeft(32, '0');

            var timeBytes = new byte[8];
            for (var j = 0; j < timeBytes.Length / 2; j++)
            {
                timeBytes[j] = Convert.ToByte(intStr.Substring(8 * j, 8), 2);
                timeBytes[j + timeBytes.Length / 2] = Convert.ToByte(floatStr.Substring(8 * j, 8), 2);
            }

            return timeBytes;
        }
    }
}