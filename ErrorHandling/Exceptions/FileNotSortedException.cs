using System;

namespace ErrorHandling.Exceptions
{
    /// <summary>
    /// Exception thrown when a file cannot be parsed because it is not sorted.
    /// </summary>
    public class FileNotSortedException : Exception
    {
        // constructor
        public FileNotSortedException(string message) : base(message) { }
    }
}
