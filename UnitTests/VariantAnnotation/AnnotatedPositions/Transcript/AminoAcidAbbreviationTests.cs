using System;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AminoAcidAbbreviationTests
    {
        [Fact]
        public void ConvertToThreeLetterAbbreviations_ExpectedResults()
        {
            // https://www.ddbj.nig.ac.jp/ddbj/code-e.html
            Assert.Equal("AlaArgAsnAspCysGlnGluGlyHisIleLeuLysMetPheProPylSerSecThrTrpTyrValAsxGlxXaaXleTer",
                AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations("ARNDCQEGHILKMFPOSUTWYVBZXJ*"));
        }

        [Fact]
        public void ConvertToThreeLetterAbbreviations_NullOrEmptyInput_ReturnEmpty()
        {
            Assert.Equal("", AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(null));
            Assert.Equal("", AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(""));
        }

        [Fact]
        public void GetThreeLetterAbbreviation_ThrowException()
        {
            Assert.Throws<NotSupportedException>(delegate { AminoAcidAbbreviation.GetThreeLetterAbbreviation('a'); });
        }
    }
}