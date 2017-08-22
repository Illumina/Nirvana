using CacheUtils.IO.Caches;
using Xunit;

namespace UnitTests.CacheUtils.IO.Caches
{
    public sealed class TranscriptCacheWriterTests
    {
        [Fact]
        public void CreateIndex_PopulatedDictionary()
        {
            var strings = new[] { "A", "B", "D", "P", "Z" };
            var dict = TranscriptCacheWriter.CreateIndex(strings);
            Assert.NotNull(dict);
            Assert.Equal(3, dict["P"]);
        }

        [Fact]
        public void CreateIndex_EmptyDictionary_WhenInputNull()
        {
            var dict = TranscriptCacheWriter.CreateIndex<string>(null);
            Assert.NotNull(dict);
            Assert.Equal(0, dict.Count);
        }
    }
}
