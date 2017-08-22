namespace VariantAnnotation.Sequence
{
    public sealed class MaskedEntry
    {
        public readonly int Begin;
        public readonly int End;

        public MaskedEntry(int begin, int end)
        {
            Begin = begin;
            End = end;
        }
    }
}