namespace VariantAnnotation.Sequence
{
    public sealed class IndexEntry
    {
        public readonly ushort RefIndex;
        public readonly long   FileOffset;

        public const int Size = 10;

        public IndexEntry(ushort refIndex, long fileOffset)
        {
            RefIndex   = refIndex;
            FileOffset = fileOffset;
        }
    }
}