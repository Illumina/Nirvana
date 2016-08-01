using System;

namespace CacheUtils.Archive
{
    public class MemoryUtilities2
    {
        /// <summary>
        /// returns the number of bytes used
        /// </summary>
        public static long NumBytesUsed(bool forceFullCollection)
        {
            return GC.GetTotalMemory(forceFullCollection);
        }
    }
}
