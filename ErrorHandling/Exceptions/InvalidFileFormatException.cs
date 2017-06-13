using System;

namespace ErrorHandling.Exceptions
{
    public sealed class InvalidFileFormatException : Exception
    {
        // constructor
        public InvalidFileFormatException(string message) : base(message) { }
    }
}
