using System;

namespace ErrorHandling.Exceptions
{
    public sealed class UserErrorException : Exception
    {
        // constructor
        public UserErrorException(string message) : base(message) { }
    }
}
