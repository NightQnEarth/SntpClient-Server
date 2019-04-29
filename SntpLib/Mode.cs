using System;

namespace SntpLib
{
    public class ModeType
    {
        private readonly string bits;

        public static readonly ModeType ClientMode = new ModeType("011");
        public static readonly ModeType ServerMode = new ModeType("100");

        private ModeType(string bits) => this.bits = bits;

        public static explicit operator ModeType(string bits)
        {
            if (bits == "011" || bits == "100")
                return new ModeType(bits);
            throw new ArgumentException();
        }

        public override string ToString() => bits;
    }
}