using System.IO;
using System.Text;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.Caches
{
    public sealed class CacheHeaderTests
    {
        [Fact]
        public void CacheHeader_EndToEnd()
        {
            const Source expectedTranscriptSource = Source.BothRefSeqAndEnsembl;
            const long expectedCreationTimeTicks  = long.MaxValue;
            const GenomeAssembly expectedAssembly = GenomeAssembly.hg19;
            const ushort expectedVepVersion       = ushort.MaxValue;

            var expectedBaseHeader   = new Header("VEP", 1, 2, expectedTranscriptSource, expectedCreationTimeTicks, expectedAssembly);
            var expectedCustomHeader = new TranscriptCacheCustomHeader(expectedVepVersion, 0);
            var expectedHeader       = new CacheHeader(expectedBaseHeader, expectedCustomHeader);

            CacheHeader observedHeader;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedHeader.Write(writer);
                }

                ms.Position = 0;
                observedHeader = CacheHeader.Read(ms);
            }

            Assert.NotNull(observedHeader);
            Assert.Equal(expectedTranscriptSource,  observedHeader.Source);
            Assert.Equal(expectedCreationTimeTicks, observedHeader.CreationTimeTicks);
            Assert.Equal(expectedAssembly,    observedHeader.Assembly);
            Assert.Equal(expectedVepVersion,        observedHeader.Custom.VepVersion);
        }
    }
}
