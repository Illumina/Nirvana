using System;

namespace ErrorHandling.Exceptions
{
    public sealed class ProcessLockedFileException : Exception
    {
        // constructor
        public ProcessLockedFileException(string message) : base(message) { }
    }
}
