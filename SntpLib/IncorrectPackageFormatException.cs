using System;

namespace SntpLib
{
    public class IncorrectPackageFormatException : Exception
    {
        public IncorrectPackageFormatException() { }

        public IncorrectPackageFormatException(string message) : base(message) { }

        public IncorrectPackageFormatException(string message, Exception inner) : base(message, inner) { }
    }
}