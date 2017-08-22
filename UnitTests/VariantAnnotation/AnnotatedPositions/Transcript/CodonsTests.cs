using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodonsTests
    {
        [Fact]
        public void Assign_WhenIntervalsNull_ReturnNull()
        {
            var cdsInterval = new NullableInterval(null, null);
            var proteinInterval = new NullableInterval(null, null);
            var codingSequence = new SimpleSequence("AAA");

            Codons.Assign("A", "G", cdsInterval, proteinInterval, codingSequence, out string refCodons,
                out string altCodons);

            Assert.Null(refCodons);
            Assert.Null(altCodons);
        }

        [Fact]
        public void Assign_SNV_SuffixLenTooBig()
        {
            var cdsInterval = new NullableInterval(89, 89);
            var proteinInterval = new NullableInterval(30, 30);

            var codingSequence = new Mock<ISequence>();
            codingSequence.SetupGet(x => x.Length).Returns(89);
            codingSequence.Setup(x => x.Substring(87, 1)).Returns("t");

            Codons.Assign("C", "T", cdsInterval, proteinInterval, codingSequence.Object, out string refCodons,
                out string altCodons);

            Assert.Equal("tC", refCodons);
            Assert.Equal("tT", altCodons);
        }

        [Fact]
        public void Assign_SNV()
        {
            var cdsInterval = new NullableInterval(24, 24);
            var proteinInterval = new NullableInterval(8, 8);

            var codingSequence = new Mock<ISequence>();
            codingSequence.SetupGet(x => x.Length).Returns(100);
            codingSequence.Setup(x => x.Substring(21, 2)).Returns("CA");

            Codons.Assign("A", "G", cdsInterval, proteinInterval, codingSequence.Object, out string refCodons,
                out string altCodons);

            Assert.Equal("caA", refCodons);
            Assert.Equal("caG", altCodons);
        }

        [Fact]
        public void Assign_MNV()
        {
            var cdsInterval = new NullableInterval(24, 28);
            var proteinInterval = new NullableInterval(8, 10);

            var codingSequence = new Mock<ISequence>();
            codingSequence.SetupGet(x => x.Length).Returns(100);
            codingSequence.Setup(x => x.Substring(21, 2)).Returns("CA");
            codingSequence.Setup(x => x.Substring(28, 2)).Returns("GG");

            Codons.Assign("GTGCT", "ACCGA", cdsInterval, proteinInterval, codingSequence.Object, out string refCodons,
                out string altCodons);

            Assert.Equal("caGTGCTgg", refCodons);
            Assert.Equal("caACCGAgg", altCodons);
        }

        [Fact]
        public void GetCodon_NullPrefixAndSuffix()
        {
            const string allele = "GAA";
            var observedResult = Codons.GetCodon(allele, "", "");
            Assert.Equal(allele, observedResult);
        }

        [Theory]
        [InlineData(3, true)]
        [InlineData(1, false)]
        public void IsTriplet(int len, bool expectedResult)
        {
            var observedResult = Codons.IsTriplet(len);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}