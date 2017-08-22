using System;
using System.Runtime.InteropServices;
using ErrorHandling.Exceptions;

namespace Compression.Utilities
{
    public static class LibraryUtilities
    {
        public static void CheckLibrary()
        {
            // check to see if we have our compression library
            try
            {
                Marshal.PtrToStringAnsi(SafeNativeMethods.GetVersion());
            }
            catch (Exception)
            {
                throw new MissingCompressionLibraryException("BlockCompression");
            }
        }

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetVersion();
        }
    }
}
