using System;
using System.IO;
using System.Runtime.InteropServices;
using ErrorHandling.Exceptions;

namespace Compression.Utilities
{
    public static class LibraryUtilities
    {
        public static void CheckLibrary()
        {
            const int expectedLibraryId = -822411574; // cafeface

            // check to see if we have our compression library
            try
            {
                int observedLibraryId = SafeNativeMethods.get_library_id();
                if (observedLibraryId != expectedLibraryId) throw new InvalidDataException("Received an incorrect library ID when validating the Block Compression library.");
            }
            catch (Exception)
            {
                throw new MissingCompressionLibraryException("BlockCompression");
            }
        }

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int get_library_id();
        }
    }
}
