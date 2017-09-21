using VariantAnnotation.Interface.Sequence;

namespace UnitTests.TestDataStructures
{
    public sealed class SimpleSequence : ISequence
    {
        private readonly string _sequence;
        private readonly int _zeroBasedStartOffset;
        public int Length => _zeroBasedStartOffset + _sequence.Length;

        public SimpleSequence(string s, int zeroBasedStartOffset = 0)
        {
            _zeroBasedStartOffset = zeroBasedStartOffset;
            _sequence = s;
        }

        public string Substring(int offset, int length)
        {
            if (offset - _zeroBasedStartOffset + length > _sequence.Length
                || offset < _zeroBasedStartOffset)
                return "";
            return _sequence.Substring(offset - _zeroBasedStartOffset, length);
        }
    }
}