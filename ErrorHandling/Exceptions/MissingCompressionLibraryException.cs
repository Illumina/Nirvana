using System;

namespace ErrorHandling.Exceptions
{
    public sealed class MissingCompressionLibraryException : Exception
    {
        // constructor
        public MissingCompressionLibraryException(string missingLibraryFilename) : base(GetErrorMessage(missingLibraryFilename)) { }

        /// <summary>
        /// returns the error message given the missing compression library filename
        /// </summary>
        private static string GetErrorMessage(string missingLibraryFilename)
        {
            return $"Unable to find the block GZip compression library ({missingLibraryFilename})";
        }
    }
}
