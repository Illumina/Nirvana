using System;

namespace ErrorHandling.Exceptions
{
    public sealed class GeneralException : Exception
    {
        // constructor
        public GeneralException(string message) : base(message) { }
    }
}
