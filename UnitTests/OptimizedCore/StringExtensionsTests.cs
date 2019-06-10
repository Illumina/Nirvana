using OptimizedCore;
using Xunit;

namespace UnitTests.OptimizedCore
{
    public sealed class StringExtensionsTests
    {
        [Theory]
        [InlineData("\tjane\tjim")]
        [InlineData("bob\tjane\t")]
        [InlineData("bob\tjane\tjim")]
        public void OptimizedSplit(string s)
        {
            var observedResult = s.OptimizedSplit('\t');
            var expectedResult = s.Split('\t');
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("123")]
        [InlineData("-123")]
        [InlineData("2147483647")]
        [InlineData("-2147483647")]
        [InlineData("4444444444")]
        [InlineData("123.3")]
        public void OptimizedParseInt32(string s)
        {
            var observedResult = s.OptimizedParseInt32();
            bool expectedFoundError = !int.TryParse(s, out int expectedResult);

            Assert.Equal(expectedFoundError, observedResult.FoundError);
            Assert.Equal(expectedResult, observedResult.Number);
        }

        [Theory]
        [InlineData("#CHROM", '#')]
        [InlineData("#CHROM", 'L')]
        public void OptimizedStartsWith(string s, char leadingChar)
        {
            bool observedResult = s.OptimizedStartsWith(leadingChar);
            bool expectedResult = s.StartsWith(leadingChar);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData("END=123")]
        [InlineData("RECOMPOSED")]
        public void OptimizedKeyValue(string s)
        {
            var observedResult = s.OptimizedKeyValue();
            var expectedResult = s.Split('=');

            Assert.Equal(expectedResult[0], observedResult.Key);
            if (expectedResult.Length == 1) Assert.Null(observedResult.Value);
            else Assert.Equal(expectedResult[1], observedResult.Value);
        }

        [Theory]
        [InlineData("<CNV>", '>')]
        [InlineData("<CNV>", 'L')]
        public void OptimizedEndsWith(string s, char leadingChar)
        {
            bool observedResult = s.OptimizedEndsWith(leadingChar);
            bool expectedResult = s.EndsWith(leadingChar);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
