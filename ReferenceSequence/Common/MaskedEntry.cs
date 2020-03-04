namespace ReferenceSequence.Common
{
    internal sealed class MaskedEntry
    {
        public readonly int Begin;
        public readonly int End;

        internal MaskedEntry(int begin, int end)
        {
            Begin = begin;
            End   = end;
        }
    }
}