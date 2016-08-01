using System;

namespace ErrorHandling.Exceptions
{
    public class CompressionException : Exception
    {
        // constructor
        public CompressionException(string message) : base(message) { }
    }
}
