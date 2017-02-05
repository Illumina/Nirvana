using System;
using System.Diagnostics;

namespace VariantAnnotation.Utilities
{
    public static class MemoryUtilities
    {
        // ReSharper disable InconsistentNaming
        private const long NumBytesInGB = 1073741824;
        private const long NumBytesInMB = 1048576;
        private const long NumBytesInKB = 1024;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// shows the peak memory usage for the current process
        /// </summary>
        public static long GetPeakMemoryUsage()
        {
            return Process.GetCurrentProcess().PeakWorkingSet64;
        }

        /// <summary>
        /// returns the number of bytes used
        /// </summary>
        public static long NumBytesUsed(bool forceFullCollection)
        {
            return GC.GetTotalMemory(forceFullCollection);
        }

        /// <summary>
        /// converts the number of bytes used to a human readable format
        /// </summary>
        public static string ToHumanReadable(long numBytes)
        {
            if (numBytes > NumBytesInGB)
            {
                var gigaBytes = numBytes / (double)NumBytesInGB;
                return $"{gigaBytes:0.000} GB";
            }

            if (numBytes > NumBytesInMB)
            {
                var megaBytes = numBytes / (double)NumBytesInMB;
                return $"{megaBytes:0.0} MB";
            }

            if (numBytes > NumBytesInKB)
            {
                var kiloBytes = numBytes / (double)NumBytesInKB;
                return $"{kiloBytes:0.0} KB";
            }

            return $"{numBytes} B";
        }
    }
}
