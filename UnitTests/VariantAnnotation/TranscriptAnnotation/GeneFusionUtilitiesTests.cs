using Moq;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using VariantAnnotation.TranscriptAnnotation;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class GeneFusionUtilitiesTests
    {
        [Fact]
        public void ComputeGeneFusions_ReturnNull_NoFusions()
        {
            var chromosome    = new Chromosome("chr1", "1", 0);
            var chromosome2   = new Chromosome("chr6", "6", 5);
            var transcriptId  = CompactId.Convert("ENST00000491426", 2);
            var transcriptId2 = CompactId.Convert("ENST00000313382", 9);

            var breakEnds = new IBreakEnd[]
            {
                new BreakEnd(chromosome, chromosome2, 31410878, 42248252, true, false)
            };

            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 144890592, 144904679, 1, 670)
            };

            var transcriptRegions2 = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 144851424, 145040002, 1, 8150)
            };

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Chromosome).Returns(chromosome);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.Gene.Symbol).Returns("PDE4DIP");
            transcript.SetupGet(x => x.Translation.CodingRegion).Returns(new CodingRegion(144890975, 144904679, 1, 287, 287));
            transcript.SetupGet(x => x.Start).Returns(144890592);
            transcript.SetupGet(x => x.End).Returns(144904679);
            transcript.SetupGet(x => x.Source).Returns(Source.Ensembl);
            transcript.SetupGet(x => x.Id).Returns(transcriptId);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions);

            var transcript2 = new Mock<ITranscript>();
            transcript2.SetupGet(x => x.Chromosome).Returns(chromosome2);
            transcript2.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript2.SetupGet(x => x.Gene.Symbol).Returns("PDE4DIP");
            transcript2.SetupGet(x => x.Translation.CodingRegion).Returns(new CodingRegion(144852458, 145039609, 394, 7116, 6723));
            transcript2.SetupGet(x => x.Start).Returns(144852458);
            transcript2.SetupGet(x => x.End).Returns(145039609);
            transcript2.SetupGet(x => x.Source).Returns(Source.Ensembl);
            transcript2.SetupGet(x => x.Id).Returns(transcriptId2);
            transcript2.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions2);

            var fusedTranscriptCandidates = new[] { transcript2.Object };

            var observedResult = GeneFusionUtilities.GetGeneFusionAnnotation(breakEnds, transcript.Object, fusedTranscriptCandidates);
            Assert.Null(observedResult);
        }

        [Fact]
        public void ComputeGeneFusions_ReturnOneGeneFusion()
        {
            var chromosome    = new Chromosome("chr1", "1", 0);
            var transcriptId  = CompactId.Convert("ENST00000367819", 2);
            var transcriptId2 = CompactId.Convert("ENST00000367818", 3);

            var breakEnds = new IBreakEnd[]
            {
                new BreakEnd(chromosome, chromosome, 168512199, 168548478, false, false),
                new BreakEnd(chromosome, chromosome, 168548478, 168512199, false, false)
            };

            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 168510003, 168510358, 210, 565),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 168510359, 168511230, 95, 210),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 168511231, 168511345, 95, 209),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 168511346, 168513141, 94, 95),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 168513142, 168513235, 1, 94)
            };

            var transcriptRegions2 = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 168545711, 168545936, 1, 226),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 168545937, 168549300, 226, 227),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 168549301, 168549415, 227, 341),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 168549416, 168550289, 341, 342),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 168550290, 168551315, 342, 1367)
            };

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Chromosome).Returns(chromosome);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.Gene.Symbol).Returns("XCL2");
            transcript.SetupGet(x => x.Translation.CodingRegion).Returns(new CodingRegion(168510190, 168513202, 34, 378, 345));
            transcript.SetupGet(x => x.Start).Returns(168510003);
            transcript.SetupGet(x => x.End).Returns(168513235);
            transcript.SetupGet(x => x.Source).Returns(Source.Ensembl);
            transcript.SetupGet(x => x.Id).Returns(transcriptId);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions);

            var transcript2 = new Mock<ITranscript>();
            transcript2.SetupGet(x => x.Chromosome).Returns(chromosome);
            transcript2.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript2.SetupGet(x => x.Gene.Symbol).Returns("XCL1");
            transcript2.SetupGet(x => x.Translation.CodingRegion).Returns(new CodingRegion(168545876, 168550458, 166, 510, 345));
            transcript2.SetupGet(x => x.Start).Returns(168545711);
            transcript2.SetupGet(x => x.End).Returns(168551315);
            transcript2.SetupGet(x => x.Source).Returns(Source.Ensembl);
            transcript2.SetupGet(x => x.Id).Returns(transcriptId2);
            transcript2.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions2);

            var fusedTranscriptCandidates = new[] { transcript2.Object };

            var expectedGeneFusions = new IGeneFusion[]
            {
                new GeneFusion(null, 1, "XCL1{ENST00000367818.3}:c.1_62-823_XCL2{ENST00000367819.2}:c.62-854_345")
            };

            var observedResult = GeneFusionUtilities.GetGeneFusionAnnotation(breakEnds, transcript.Object, fusedTranscriptCandidates);
            Assert.Single(observedResult.GeneFusions);
            Assert.Equal(expectedGeneFusions[0].Exon, observedResult.GeneFusions[0].Exon);
            Assert.Equal(expectedGeneFusions[0].Intron, observedResult.GeneFusions[0].Intron);
            Assert.Equal(expectedGeneFusions[0].HgvsCoding, observedResult.GeneFusions[0].HgvsCoding);
        }
    }
}