namespace ReferenceSequence.Common
{
    internal sealed class IndexEntry
    {
        public readonly ushort RefIndex;
        public readonly long   FileOffset;

        public const int Size = 10;

        internal IndexEntry(ushort refIndex, long fileOffset)
        {
            RefIndex   = refIndex;
            FileOffset = fileOffset;
        }
    }
}