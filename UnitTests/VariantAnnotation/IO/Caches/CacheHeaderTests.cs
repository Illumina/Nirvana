using System.IO;
using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.Caches
{
    public sealed class CacheHeaderTests
    {
        [Fact]
        public void CacheHeader_EndToEnd()
        {
            const Source expectedTranscriptSource       = Source.BothRefSeqAndEnsembl;
            const long expectedCreationTimeTicks        = long.MaxValue;
            const GenomeAssembly expectedGenomeAssembly = GenomeAssembly.hg19;
            const ushort expectedVepVersion             = ushort.MaxValue;

            var expectedCustomHeader = new TranscriptCacheCustomHeader(expectedVepVersion, 0);
            var expectedHeader = new CacheHeader("VEP", 1, 2, expectedTranscriptSource, expectedCreationTimeTicks,
                expectedGenomeAssembly, expectedCustomHeader);

            CacheHeader observedHeader;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedHeader.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedHeader = CacheHeader.Read(reader, TranscriptCacheCustomHeader.Read) as CacheHeader;
                }
            }

            Assert.NotNull(observedHeader);
            Assert.Equal(expectedTranscriptSource, observedHeader.TranscriptSource);
            Assert.Equal(expectedCreationTimeTicks, observedHeader.CreationTimeTicks);
            Assert.Equal(expectedGenomeAssembly, observedHeader.GenomeAssembly);

            var observedCustomHeader = observedHeader.CustomHeader as TranscriptCacheCustomHeader;
            Assert.NotNull(observedCustomHeader);
            Assert.Equal(expectedVepVersion, observedCustomHeader.VepVersion);
        }
    }
}
