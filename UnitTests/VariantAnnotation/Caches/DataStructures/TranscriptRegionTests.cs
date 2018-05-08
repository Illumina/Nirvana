using System.IO;
using System.Text;
using CacheUtils.TranscriptCache.Comparers;
using IO;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptRegionTests
    {
        [Fact]
        public void TranscriptRegion_EndToEnd()
        {
            var expectedResults = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 13, 100, 200, 300, 400),
                new TranscriptRegion(TranscriptRegionType.Gap, 0, 120, 230, 10, 20),
                new TranscriptRegion(TranscriptRegionType.Intron, 14, 130, 230, 330, 430)
            };

            var observedResults = new ITranscriptRegion[expectedResults.Length];

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    foreach(var region in expectedResults) region.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new BufferedBinaryReader(ms))
                {
                    for (int i = 0; i < expectedResults.Length; i++)
                    {
                        observedResults[i] = TranscriptRegion.Read(reader);
                    }
                }
            }

            var comparer = new TranscriptRegionComparer();
            Assert.Equal(expectedResults, observedResults, comparer);
        }
    }
}
