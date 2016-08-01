using Xunit;
using VariantAnnotation.Utilities;

namespace UnitTests.Utilities
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("BOB", "SALLY", 0)]
        [InlineData("BOB","BARRY", 1)]        
        [InlineData("BOB", "BOB", 3)]
        [InlineData(null, "BOB", 0)]
        [InlineData("BOB", null, 0)]
        public void CommonPrefixLength(string a, string b, int expectedLength)
        {
            var observedLength = a.CommonPrefixLength(b);
            Assert.Equal(expectedLength, observedLength);
        }

        [Theory]
        [InlineData("BOB", "SALLY", 0)]
        [InlineData("BOB", "JOB", 2)]
        [InlineData("BOB", "BOB", 3)]
        [InlineData(null, "BOB", 0)]
        [InlineData("BOB", null, 0)]
        public void CommonSuffixLength(string a, string b, int expectedLength)
        {
            var observedLength = a.CommonSuffixLength(b);
            Assert.Equal(expectedLength, observedLength);
        }

        [Theory]
        [InlineData("AlaGlnHisVal", "Ala")]
        [InlineData(null, "")]
        [InlineData("Al", "")]
        public void FirstAminoAcid3(string a, string expectedAminoAcid)
        {
            var observedAminoAcid = a.FirstAminoAcid3();
            Assert.Equal(expectedAminoAcid, observedAminoAcid);
        }

        [Theory]
        [InlineData("AlaGlnHisVal", "Val")]
        [InlineData(null, "")]
        [InlineData("Al", "")]
        public void LastAminoAcid3(string a, string expectedAminoAcid)
        {
            var observedAminoAcid = a.LastAminoAcid3();
            Assert.Equal(expectedAminoAcid, observedAminoAcid);
        }
    }
}
