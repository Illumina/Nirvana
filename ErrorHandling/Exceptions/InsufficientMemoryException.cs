using System;

namespace ErrorHandling.Exceptions
{
    /// <summary>
    /// Exception thrown when a file cannot be parsed because its format is not valid.
    /// </summary>
    public class InsufficientMemoryException : Exception
    {
        // constructor
        public InsufficientMemoryException(string message) : base(message) { }
    }
}
