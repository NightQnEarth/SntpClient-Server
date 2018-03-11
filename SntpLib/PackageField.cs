namespace SntpLib
{
    public class PackageField
    {
        public readonly int PackageStartByte;
        public byte[] ByteArray;

        public PackageField(int packageStartByte, byte[] byteArray)
        {
            ByteArray = byteArray;
            PackageStartByte = packageStartByte;
        }
    }
}