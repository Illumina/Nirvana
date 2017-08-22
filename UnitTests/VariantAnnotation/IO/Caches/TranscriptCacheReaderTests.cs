using System.Collections.Generic;
using System.IO;
using System.Text;
using CacheUtils.IO.Caches;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.Caches
{
    public sealed class TranscriptCacheReaderTests
    {
        private readonly IChromosomeInterval _transcriptSearchInterval;
        private readonly IChromosomeInterval _regulatoryRegionSearchInterval;

        private readonly Dictionary<ushort, IChromosome> _refIndexToChromosome;
        private readonly CacheHeader _expectedHeader;

        private readonly ITranscript[]       _expectedTranscripts;
        private readonly IRegulatoryRegion[] _expectedRegulatoryRegions;
        private readonly IGene[]             _expectedGenes;
        private readonly IInterval[]         _expectedIntrons;
        private readonly IInterval[]         _expectedMirnas;
        private readonly string[]            _expectedPeptideSeqs;

        public TranscriptCacheReaderTests()
        {
            var chr3              = new Chromosome("chr3", "3", 2);
            _refIndexToChromosome = new Dictionary<ushort, IChromosome> { [chr3.Index] = chr3 };

            _transcriptSearchInterval       = new ChromosomeSearchInterval(chr3, 100, 200);
            _regulatoryRegionSearchInterval = new ChromosomeSearchInterval(chr3, 1000, 2000);

            var expectedCustomHeader = new TranscriptCacheCustomHeader(1, 2);
            _expectedHeader          = new CacheHeader("test", 2, 3, Source.BothRefSeqAndEnsembl, 4, GenomeAssembly.GRCh38, expectedCustomHeader);

            _expectedIntrons    = new IInterval[1];
            _expectedIntrons[0] = new Interval(100, 200);

            _expectedMirnas    = new IInterval[2];
            _expectedMirnas[0] = _expectedIntrons[0];
            _expectedMirnas[1] = new Interval(300, 400);

            _expectedPeptideSeqs = new[] {"MASE*"};

            _expectedGenes = new IGene[1];
            _expectedGenes[0] = new Gene(chr3, 100, 200, true, "TP53", 300, CompactId.Convert("7157"),
                CompactId.Convert("ENSG00000141510"), 500);

            _expectedRegulatoryRegions = new IRegulatoryRegion[2];
            _expectedRegulatoryRegions[0] = new RegulatoryRegion(chr3, 1200, 1300, CompactId.Convert("123"), RegulatoryElementType.enhancer);
            _expectedRegulatoryRegions[1] = new RegulatoryRegion(chr3, 1250, 1450, CompactId.Convert("456"), RegulatoryElementType.enhancer);

            _expectedTranscripts = GetTranscripts(chr3);
        }

        [Fact]
        public void TranscriptCacheReader_EndToEnd()
        {
            TranscriptCache observedCache;

            using (var ms = new MemoryStream())
            {
                using (var writer = new TranscriptCacheWriter(ms, _expectedHeader, true))
                {
                    writer.Write(_expectedTranscripts, _expectedRegulatoryRegions, _expectedGenes, _expectedIntrons,
                        _expectedMirnas, _expectedPeptideSeqs);
                }

                ms.Position = 0;

                using (var reader = new TranscriptCacheReader(ms, GenomeAssembly.GRCh38, 25))
                {
                    observedCache = reader.Read(_refIndexToChromosome);
                }
            }

            Assert.NotNull(observedCache);

            var overlappingTranscripts = observedCache.GetOverlappingFlankingTranscripts(_transcriptSearchInterval);
            Assert.Equal(1, overlappingTranscripts.Length);

            var overlappingRegulatoryRegions = observedCache.GetOverlappingRegulatoryRegions(_regulatoryRegionSearchInterval);
            Assert.Equal(2, overlappingRegulatoryRegions.Length);
        }

        [Fact]
        public void ReadItems_EndToEnd()
        {
            var expectedStrings = new[] { "Huey", "Duey", "Louie" };
            string[] observedStrings;

            using (var ms = new MemoryStream())
            {
                // ReSharper disable AccessToDisposedClosure
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    TranscriptCacheWriter.WriteItems(writer, expectedStrings, x => writer.WriteOptAscii(x));
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedStrings = TranscriptCacheReader.ReadItems(reader, () => reader.ReadAsciiString());
                }
                // ReSharper restore AccessToDisposedClosure
            }

            Assert.NotNull(observedStrings);
            Assert.Equal(expectedStrings, observedStrings);
        }

        [Fact]
        public void CheckGuard_InvalidGuard()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true)) writer.Write(7);
                    ms.Position = 0;
                    using (var reader = new ExtendedBinaryReader(ms)) TranscriptCacheReader.CheckGuard(reader);
                }
            });
        }

        private sealed class ChromosomeSearchInterval : IChromosomeInterval
        {
            public int Start { get; }
            public int End { get; }
            public IChromosome Chromosome { get; }

            public ChromosomeSearchInterval(IChromosome chromosome, int start, int end)
            {
                Chromosome = chromosome;
                Start      = start;
                End        = end;
            }
        }

        private ITranscript[] GetTranscripts(IChromosome chromosome)
        {
            var cdnaMaps = new ICdnaCoordinateMap[1];
            cdnaMaps[0]  = new CdnaCoordinateMap(100, 199, 300, 399);

            var transcripts = new ITranscript[1];
            transcripts[0] = new Transcript(chromosome, 120, 180,
                CompactId.Convert("789"), 0, null, BioType.IG_D_gene, _expectedGenes[0],
                0, 0, false, _expectedIntrons, _expectedMirnas,
                cdnaMaps, -1, -1, Source.None);

            return transcripts;
        }
    }
}
