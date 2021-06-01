using System.Collections.Generic;
using System.IO;
using Intervals;
using SAUtils.CosmicGeneFusions.Cache;
using UnitTests.MockedData;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.Cache
{
    public sealed class TranscriptCacheTests
    {
        [Theory]
        [InlineData("ENST00000646891.1", "ENSG00000157764", "BRAF")]
        [InlineData("ENST00000242365.4", "ENSG00000122778", "KIAA1549")]
        [InlineData("ENST00000311979.3", "ENSG00000172660", "TAF15")]
        [InlineData("ENST00000529193.1", "ENSG00000157613", "CREB3L1")]
        [InlineData("ENST00000312675.4", "ENSG00000145012", "LPP")]
        [InlineData("ENST00000556625.1", "ENSG00000258389", "DUX4")]
        public void HandleMissingTranscripts_ExpectedResults(string transcriptId, string expectedGeneId, string expectedGeneSymbol)
        {
            (string actualGeneId, string actualGeneSymbol) = TranscriptCache.HandleMissingTranscripts(transcriptId);
            Assert.Equal(expectedGeneId,     actualGeneId);
            Assert.Equal(expectedGeneSymbol, actualGeneSymbol);
        }

        [Fact]
        public void HandleMissingTranscripts_UnknownTranscriptId_ThrowException()
        {
            Assert.Throws<InvalidDataException>(delegate { TranscriptCache.HandleMissingTranscripts("ABC"); });
        }

        [Fact]
        public void GetTranscriptIdToTranscript()
        {
            var chr1 = new IntervalArray<ITranscript>(new Interval<ITranscript>[]
            {
                new(Transcripts.ENST00000290663.Start, Transcripts.ENST00000290663.End, Transcripts.ENST00000290663),
                new(Transcripts.ENST00000370673.Start, Transcripts.ENST00000370673.End, Transcripts.ENST00000370673),
                new(Transcripts.ENST00000427819.Start, Transcripts.ENST00000427819.End, Transcripts.ENST00000427819)
            });

            var chr2 = new IntervalArray<ITranscript>(new Interval<ITranscript>[]
            {
                new(Transcripts.ENST00000615053.Start, Transcripts.ENST00000615053.End, Transcripts.ENST00000615053),
                new(Transcripts.ENST00000347849.Start, Transcripts.ENST00000347849.End, Transcripts.ENST00000347849)
            });

            var transcriptIntervalArrays = new IntervalArray<ITranscript>[ChromosomeUtilities.RefIndexToChromosome.Count];
            transcriptIntervalArrays[ChromosomeUtilities.Chr1.Index] = chr1;
            transcriptIntervalArrays[ChromosomeUtilities.Chr2.Index] = chr2;

            Dictionary<string, ITranscript> idToTranscript = TranscriptCache.GetTranscriptIdToTranscript(transcriptIntervalArrays);

            Assert.Equal(10, idToTranscript.Count);
            Assert.True(idToTranscript.ContainsKey("ENST00000290663"));
            Assert.True(idToTranscript.ContainsKey("ENST00000290663.10"));
        }
    }
}