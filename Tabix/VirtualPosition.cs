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
    }
}
