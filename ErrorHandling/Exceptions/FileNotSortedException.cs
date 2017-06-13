using System;

namespace ErrorHandling.Exceptions
{
    public sealed class FileNotSortedException : Exception
    {
        // constructor
        public FileNotSortedException(string message) : base(message) { }
    }
}
