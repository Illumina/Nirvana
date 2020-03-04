using Genome;
using Intervals;
using ReferenceSequence.Common;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
	public sealed class CompressedSequenceTests
	{
		private readonly ReferenceSequence.Common.Sequence _sequence;
		private const int NumBases = 53;

		public CompressedSequenceTests()
		{
            _sequence = new ReferenceSequence.Common.Sequence { Assembly = GenomeAssembly.hg19 };

            // create the following sequence: NNATGTTTCCACTTTCTCCTCATTAGANNNTAACGAATGGGTGATTTCCCTAN
            var twoBitBuffer = new byte[] { 14, 42, 93, 169, 150, 122, 204, 11, 211, 224, 35, 169, 91, 0 };

			var maskedIntervals = new Interval<MaskedEntry>[3];
			maskedIntervals[0]  = new Interval<MaskedEntry>(0, 1, new MaskedEntry(0, 1));
			maskedIntervals[1]  = new Interval<MaskedEntry>(27, 29, new MaskedEntry(27, 29));
			maskedIntervals[2]  = new Interval<MaskedEntry>(52, 52, new MaskedEntry(52, 52));

			var maskedIntervalArray = new IntervalArray<MaskedEntry>(maskedIntervals);
            _sequence.Set(NumBases, 0, twoBitBuffer, maskedIntervalArray, null);
        }

	    [Fact]
	    public void Assembly_hg19()
	    {
	        Assert.Equal(GenomeAssembly.hg19, _sequence.Assembly);
	    }

        [Fact]
	    public void GetNumBufferBytes()
        {
            const int expectedNumBufferBytes = 25;
            var observedNumBufferBytes = ReferenceSequence.Common.Sequence.GetNumBufferBytes(97);
            Assert.Equal(expectedNumBufferBytes, observedNumBufferBytes);
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
			var observedSubstring = _sequence.Substring(offset, length);
			Assert.Equal(expectedSubstring, observedSubstring);
		}		
	}
}