using System;

namespace ErrorHandling.Exceptions
{
    public class ProcessLockedFileException : Exception
    {
        // constructor
        public ProcessLockedFileException(string message) : base(message) { }
    }
}
