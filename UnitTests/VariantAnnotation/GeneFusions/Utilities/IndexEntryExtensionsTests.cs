using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.Utilities
{
    public sealed class IndexEntryExtensionsTests
    {
        private readonly GeneFusionIndexEntry[] _indexEntries;

        public IndexEntryExtensionsTests()
        {
            _indexEntries = new GeneFusionIndexEntry[]
            {
                new(1000, 1),
                new(1001, 2),
                new(2000, 3),
                new(3000, 4),
                new(3100, 5)
            };
        }

        [Theory]
        [InlineData(1000, 1)]
        [InlineData(1001, 2)]
        [InlineData(2000, 3)]
        [InlineData(3000, 4)]
        [InlineData(3100, 5)]
        public void GetIndex_Matches_ExpectedResults(ulong geneKey, ushort expectedIndex)
        {
            ushort? actualIndex = _indexEntries.GetIndex(geneKey);
            Assert.NotNull(actualIndex);
            Assert.Equal(expectedIndex, actualIndex);
        }
        
        [Theory]
        [InlineData(100)]
        [InlineData(1002)]
        [InlineData(4000)]
        public void GetIndex_NotFound_ReturnNull(ulong geneKey)
        {
            ushort? actualIndex = _indexEntries.GetIndex(geneKey);
            Assert.Null(actualIndex);
        }
    }
}