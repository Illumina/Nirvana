using Genome;
using Intervals;
using ReferenceSequence.Common;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
	public sealed class CompressedSequenceTests
	{
		private readonly CompressedSequence _compressedSequence;
		private const int NumBases = 53;

		public CompressedSequenceTests()
		{
            _compressedSequence = new CompressedSequence { Assembly = GenomeAssembly.hg19 };

            // create the following sequence: NNATGTTTCCACTTTCTCCTCATTAGANNNTAACGAATGGGTGATTTCCCTAN
            var twoBitBuffer = new byte[] { 14, 42, 93, 169, 150, 122, 204, 11, 211, 224, 35, 169, 91, 0 };

			var maskedIntervals = new Interval[3];
			maskedIntervals[0]  = new Interval(0, 1);
			maskedIntervals[1]  = new Interval(27, 29);
			maskedIntervals[2]  = new Interval(52, 52);

			IntervalArray<Interval>.Interval[] newIntervals = IntervalUtilities.CreateIntervals(maskedIntervals);

			var maskedIntervalArray = new IntervalArray<Interval>(newIntervals);
            _compressedSequence.Set(NumBases, 0, twoBitBuffer, maskedIntervalArray, null);
        }

	    [Fact]
	    public void Assembly_hg19()
	    {
	        Assert.Equal(GenomeAssembly.hg19, _compressedSequence.Assembly);
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