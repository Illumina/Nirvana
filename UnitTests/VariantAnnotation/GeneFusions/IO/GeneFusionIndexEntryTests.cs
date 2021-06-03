using VariantAnnotation.GeneFusions.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.IO
{
    public sealed class GeneFusionIndexEntryTests
    {
        [Theory]
        [InlineData(1000, 1)]
        [InlineData(2000, 0)]
        [InlineData(3000, -1)]
        public void Compare_ExpectedResults(ulong otherGeneKey, int expectedResult)
        {
            var indexEntry   = new GeneFusionIndexEntry(2000, 0);
            int actualResult = indexEntry.Compare(otherGeneKey);
            Assert.Equal(expectedResult, actualResult);
        }
    }
}