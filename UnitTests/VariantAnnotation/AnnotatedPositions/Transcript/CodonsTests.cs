using Genome;
using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions.Transcript;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodonsTests
    {
        [Fact]
        public void Assign_WhenIntervalsNull_ReturnNull()
        {
            var sequence = new SimpleSequence("AAA");
            var codons = Codons.GetCodons("G", -1, -1, -1, -1, sequence);

            Assert.Equal("", codons.Reference);
            Assert.Equal("", codons.Alternate);
        }

        [Fact]
        public void Assign_SNV_SuffixLenTooBig()
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(89);
            sequence.Setup(x => x.Substring(87, 1)).Returns("t");
            sequence.Setup(x => x.Substring(88, 1)).Returns("C");

            var codons = Codons.GetCodons("T", 89, 89, 30, 30, sequence.Object);

            Assert.Equal("tC", codons.Reference);
            Assert.Equal("tT", codons.Alternate);
        }

        [Fact]
        public void Assign_SNV()
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(100);
            sequence.Setup(x => x.Substring(21, 2)).Returns("CA");
            sequence.Setup(x => x.Substring(23, 1)).Returns("A");

            var codons = Codons.GetCodons("G", 24, 24, 8, 8, sequence.Object);

            Assert.Equal("caA", codons.Reference);
            Assert.Equal("caG", codons.Alternate);
        }

        [Fact]
        public void Assign_MNV()
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(100);
            sequence.Setup(x => x.Substring(21, 2)).Returns("CA");
            sequence.Setup(x => x.Substring(28, 2)).Returns("GG");
            sequence.Setup(x => x.Substring(23, 5)).Returns("GTGCT");

            var codons = Codons.GetCodons("ACCGA", 24, 28, 8, 10, sequence.Object);

            Assert.Equal("caGTGCTgg", codons.Reference);
            Assert.Equal("caACCGAgg", codons.Alternate);
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

        [Theory]
        [InlineData(-33, 4, -11, 2, "ACGTca")]
        [InlineData(95, 101, 32, 34, "gGCTGA")]
        public void GetCodons_OutOfRangeIndexes_Adjusted(int cdsStart, int cdsEnd, int proteinBegin, int proteinEnd, string expectedRefCodons)
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(99);
            sequence.Setup(x => x.Substring(0, 0)).Returns("");
            sequence.Setup(x => x.Substring(0, 4)).Returns("ACGT");
            sequence.Setup(x => x.Substring(4, 2)).Returns("CA");
            sequence.Setup(x => x.Substring(94, 5)).Returns("GCTGA");
            sequence.Setup(x => x.Substring(93, 1)).Returns("G");

            var codons = Codons.GetCodons("", cdsStart, cdsEnd, proteinBegin, proteinEnd, sequence.Object);

            Assert.Equal(expectedRefCodons, codons.Reference);
        }
    }
}