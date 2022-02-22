using System;
using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class CodonsTests
    {
        [Fact]
        public void GetCodons_UndefinedInterval_ReturnEmpty()
        {
            var sequence = "AAA".AsSpan();
            (string actualRefCodons, string actualAltCodons) = Codons.GetCodons("G", -1, -1, -1, -1, sequence);

            Assert.Equal("", actualRefCodons);
            Assert.Equal("", actualAltCodons);
        }

        [Fact]
        public void GetCodons_SNV_SuffixLenTooBig()
        {
            Span<char>         sequence    = stackalloc char[89];
            ReadOnlySpan<char> contentSpan = "tC".AsSpan();
            contentSpan.CopyTo(sequence.Slice(87));

            (string actualRefCodons, string actualAltCodons) = Codons.GetCodons("T", 89, 89, 30, 30, sequence);

            Assert.Equal("tC", actualRefCodons);
            Assert.Equal("tT", actualAltCodons);
        }

        [Fact]
        public void GetCodons_SNV_ExpectedResults()
        {
            Span<char>         sequence    = stackalloc char[100];
            ReadOnlySpan<char> contentSpan = "CAA".AsSpan();
            contentSpan.CopyTo(sequence.Slice(21));

            (string actualRefCodons, string actualAltCodons) = Codons.GetCodons("G", 24, 24, 8, 8, sequence);

            Assert.Equal("caA", actualRefCodons);
            Assert.Equal("caG", actualAltCodons);
        }

        [Fact]
        public void GetCodons_MNV_ExpectedResults()
        {
            Span<char>         sequence     = stackalloc char[100];
            ReadOnlySpan<char> contentSpan  = "CAGTGCT".AsSpan();
            ReadOnlySpan<char> contentSpan2 = "GG".AsSpan();
            contentSpan.CopyTo(sequence.Slice(21));
            contentSpan2.CopyTo(sequence.Slice(28));

            (string actualRefCodons, string actualAltCodons) =
                Codons.GetCodons("ACCGA", 24, 28, 8, 10, sequence);

            Assert.Equal("caGTGCTgg", actualRefCodons);
            Assert.Equal("caACCGAgg", actualAltCodons);
        }

        [Fact]
        public void GetCodon_NullPrefixAndSuffix()
        {
            const string expectedResult = "GAA";
            string       observedResult = Codons.GetCodon(expectedResult, "", "");
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(3, true)]
        [InlineData(1, false)]
        public void IsTriplet_ExpectedResults(int len, bool expectedResult)
        {
            bool actualResult = Codons.IsTriplet(len);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(-33, 4, -11, 2, "ACGTca")]
        [InlineData(95, 101, 32, 34, "gGCTGA")]
        public void GetCodons_OutOfRangeIndexes_Adjusted(int cdsStart, int cdsEnd, int proteinBegin, int proteinEnd,
            string expectedRefCodons)
        {
            Span<char>         sequence     = stackalloc char[99];
            ReadOnlySpan<char> contentSpan  = "ACGTCA".AsSpan();
            ReadOnlySpan<char> contentSpan2 = "GGCTGA".AsSpan();
            contentSpan.CopyTo(sequence);
            contentSpan2.CopyTo(sequence.Slice(93));

            (string actualRefCodons, _) =
                Codons.GetCodons("", cdsStart, cdsEnd, proteinBegin, proteinEnd, sequence);
            Assert.Equal(expectedRefCodons, actualRefCodons);
        }
    }
}