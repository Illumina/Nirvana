using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
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

            var maskedIntervalTree = new IntervalTree<MaskedEntry>
            {
                new IntervalTree<MaskedEntry>.Interval(string.Empty, 0, 1, new MaskedEntry(0, 1)),
                new IntervalTree<MaskedEntry>.Interval(string.Empty, 27, 29, new MaskedEntry(27, 29)),
                new IntervalTree<MaskedEntry>.Interval(string.Empty, 52, 52, new MaskedEntry(52, 52))
            };

            _compressedSequence.Set(NumBases, buffer, maskedIntervalTree);
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
            string observedSubstring = _compressedSequence.Substring(offset, length);
            Assert.Equal(expectedSubstring, observedSubstring);
        }

        [Fact]
        public void GenomeAssemblyTest()
        {
            const GenomeAssembly expectedGenomeAssembly = GenomeAssembly.GRCh37;
            var observedGenomeAssembly = AnnotationLoader.Instance.GenomeAssembly;
            Assert.Equal(expectedGenomeAssembly, observedGenomeAssembly);
        }
    }
}
