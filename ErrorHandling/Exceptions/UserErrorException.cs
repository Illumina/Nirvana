using System;

namespace ErrorHandling.Exceptions
{
    public class UserErrorException : Exception
    {
        // constructor
        public UserErrorException(string message) : base(message) { }
    }
}
