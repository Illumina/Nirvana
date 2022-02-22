using System.Collections.Generic;
using Cache.Data;
using UnitTests.TestUtilities;
using UnitTests.VariantAnnotation.Utilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class GeneFusionUtilitiesTests
    {
        [Fact]
        public void ComputeGeneFusions_ReturnNull_NoFusions()
        {
            const string transcriptId  = "ENST00000491426.2";
            const string transcriptId2 = "ENST00000313382.9";

            var breakEnds = new IBreakEnd[]
            {
                new BreakEnd(ChromosomeUtilities.Chr1, ChromosomeUtilities.Chr6, 31410878, 42248252, true, false)
            };

            var transcriptRegions = new TranscriptRegion[]
            {
                new(144890592, 144904679, 1, 670, TranscriptRegionType.Exon, 1, null)
            };

            var transcriptRegions2 = new TranscriptRegion[]
            {
                new(144851424, 145040002, 1, 8150, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion =
                new CodingRegion(144890975, 144904679, 1, 287, string.Empty, string.Empty, 0, 0, 0, null, null);
            var codingRegion2 = new CodingRegion(144852458, 145039609, 394, 7116, string.Empty, string.Empty, 0, 0, 0,
                null, null);

            var transcript = TranscriptMocker.GetTranscript(true, "PDE4DIP", transcriptRegions, codingRegion,
                ChromosomeUtilities.Chr1, 144890592, 144904679, Source.Ensembl, transcriptId);
            var transcript2 = TranscriptMocker.GetTranscript(true, "PDE4DIP", transcriptRegions2, codingRegion2,
                ChromosomeUtilities.Chr6, 144852458, 145039609, Source.Ensembl, transcriptId2);

            var fusedTranscriptCandidates = new HashSet<Transcript> {transcript2};

            var observedResult =
                GeneFusionUtilities.GetGeneFusionAnnotation(breakEnds, transcript, fusedTranscriptCandidates);
            Assert.Null(observedResult);
        }

        [Fact]
        public void ComputeGeneFusions_ReturnOneGeneFusion()
        {
            var transcriptId  = "ENST00000367819.2";
            var transcriptId2 = "ENST00000367818.3";

            var breakEnds = new IBreakEnd[]
            {
                new BreakEnd(ChromosomeUtilities.Chr1, ChromosomeUtilities.Chr1, 168512199, 168548478, false, false),
                new BreakEnd(ChromosomeUtilities.Chr1, ChromosomeUtilities.Chr1, 168548478, 168512199, false, false)
            };

            var transcriptRegions = new TranscriptRegion[]
            {
                new(168510003, 168510358, 210, 565, TranscriptRegionType.Exon, 3, null),
                new(168510359, 168511230, 95, 210, TranscriptRegionType.Intron, 2, null),
                new(168511231, 168511345, 95, 209, TranscriptRegionType.Exon, 2, null),
                new(168511346, 168513141, 94, 95, TranscriptRegionType.Intron, 1, null),
                new(168513142, 168513235, 1, 94, TranscriptRegionType.Exon, 1, null)
            };

            var transcriptRegions2 = new TranscriptRegion[]
            {
                new(168545711, 168545936, 1, 226, TranscriptRegionType.Exon, 1, null),
                new(168545937, 168549300, 226, 227, TranscriptRegionType.Intron, 1, null),
                new(168549301, 168549415, 227, 341, TranscriptRegionType.Exon, 2, null),
                new(168549416, 168550289, 341, 342, TranscriptRegionType.Intron, 2, null),
                new(168550290, 168551315, 342, 1367, TranscriptRegionType.Exon, 3, null)
            };

            var codingRegion = new CodingRegion(168510190, 168513202, 34, 378, string.Empty, string.Empty, 0, 0, 0, null,
                null);
            var codingRegion2 = new CodingRegion(168545876, 168550458, 166, 510, string.Empty, string.Empty, 0, 0, 0, null,
                null);

            var transcript = TranscriptMocker.GetTranscript(true, "XCL2", transcriptRegions, codingRegion,
                ChromosomeUtilities.Chr1, 168510003, 168513235, Source.Ensembl, transcriptId);
            var transcript2 = TranscriptMocker.GetTranscript(false, "XCL1", transcriptRegions2, codingRegion2,
                ChromosomeUtilities.Chr1, 168545711, 168551315, Source.Ensembl, transcriptId2);
            
            var fusedTranscriptCandidates = new HashSet<Transcript> {transcript2};

            var expectedGeneFusions = new GeneFusion[]
            {
                new(null, 1, "XCL1{ENST00000367818.3}:c.1_62-823_XCL2{ENST00000367819.2}:c.62-854_345")
            };

            var observedResult =
                GeneFusionUtilities.GetGeneFusionAnnotation(breakEnds, transcript, fusedTranscriptCandidates);
            Assert.Single(observedResult.GeneFusions);
            Assert.Equal(expectedGeneFusions[0].Exon, observedResult.GeneFusions[0].Exon);
            Assert.Equal(expectedGeneFusions[0].Intron, observedResult.GeneFusions[0].Intron);
            Assert.Equal(expectedGeneFusions[0].HgvsCoding, observedResult.GeneFusions[0].HgvsCoding);
        }
    }
}