namespace Tabix
{
    public static class VirtualPosition
    {
        public static (long FileOffset, int BlockOffset) From(long virtualPosition)
        {
            unchecked
            {
                return ((virtualPosition >> 16) & 0xFFFFFFFFFFFFL, (int)(virtualPosition & 0xffff));
            }
        }

        public static long To(long fileOffset, int blockOffset) => (fileOffset << 16) | ((long)blockOffset & 0xffff);
    }
}
