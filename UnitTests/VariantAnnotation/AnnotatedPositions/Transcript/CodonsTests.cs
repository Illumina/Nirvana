using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodonsTests
    {
        [Fact]
        public void Assign_WhenIntervalsNull_ReturnNull()
        {
            var sequence = new SimpleSequence("AAA");
            var codons = Codons.GetCodons("A", "G", -1, -1, -1, -1, sequence);

            Assert.Equal("", codons.Reference);
            Assert.Equal("", codons.Alternate);
        }

        [Fact]
        public void Assign_SNV_SuffixLenTooBig()
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(89);
            sequence.Setup(x => x.Substring(87, 1)).Returns("t");

            var codons = Codons.GetCodons("C", "T", 89, 89, 30, 30, sequence.Object);

            Assert.Equal("tC", codons.Reference);
            Assert.Equal("tT", codons.Alternate);
        }

        [Fact]
        public void Assign_SNV()
        {
            var sequence = new Mock<ISequence>();
            sequence.SetupGet(x => x.Length).Returns(100);
            sequence.Setup(x => x.Substring(21, 2)).Returns("CA");

            var codons = Codons.GetCodons("A", "G", 24, 24, 8, 8, sequence.Object);

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

            var codons = Codons.GetCodons("GTGCT", "ACCGA", 24, 28, 8, 10, sequence.Object);

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
    }
}