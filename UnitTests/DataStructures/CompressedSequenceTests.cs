using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    public class CompressedSequenceTests
    {
        #region members

        private readonly ICompressedSequence _compressedSequence;

        private const int NumBases = 53;

        #endregion

        // constructor
        public CompressedSequenceTests()
        {
            _compressedSequence = new CompressedSequence();

            // create the following sequence: NNATGTTTCCACTTTCTCCTCATTAGANNNTAACGAATGGGTGATTTCCCTAN
            var buffer = new byte[] { 14, 42, 93, 169, 150, 122, 204, 11, 211, 224, 35, 169, 91, 0 };

            var maskedIntervals = new IntervalArray<MaskedEntry>.Interval[3];
            maskedIntervals[0] = new IntervalArray<MaskedEntry>.Interval(0, 1, new MaskedEntry(0, 1));
            maskedIntervals[1] = new IntervalArray<MaskedEntry>.Interval(27, 29, new MaskedEntry(27, 29));
            maskedIntervals[2] = new IntervalArray<MaskedEntry>.Interval(52, 52, new MaskedEntry(52, 52));

            var maskedIntervalArray = new IntervalArray<MaskedEntry>(maskedIntervals);

            _compressedSequence.Set(NumBases, buffer, maskedIntervalArray);
        }

        [Theory]
        [InlineData(23, 5, "TAGAN")]
        [InlineData(0, 5, "NNATG")]
        [InlineData(-1, 5, null)]
        [InlineData(48, 5, "CCTAN")]
        [InlineData(49, 5, "CTAN")]
        [InlineData(53, 5, null)]
        [InlineData(23, 0, null)]
        public void Substring(int offset, int length, string expectedSubstring)
        {
            var observedSubstring = _compressedSequence.Substring(offset, length);
            Assert.Equal(expectedSubstring, observedSubstring);
        }
    }
}
