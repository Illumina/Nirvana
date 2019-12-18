using System.Collections.Generic;
using Genome;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.TranscriptAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class GeneFusionUtilitiesTests
    {
        private readonly IChromosome _chr2 = new Chromosome("chr2", "2", 1);

        private readonly ITranscript _enst00000425361;
        private readonly ITranscript _enst00000427024;
        private readonly ITranscript[] _originTranscripts;
        private readonly ITranscript[] _partnerTranscripts;

        public GeneFusionUtilitiesTests()
        {
            IGene mzt2BGene = new Gene(_chr2, 0, 0, false, "MZT2B", 0, CompactId.Empty, CompactId.Empty);
            IGene mzt2AGene = new Gene(_chr2, 0, 0, true, "MZT2A", 0, CompactId.Empty, CompactId.Empty);

            var transcriptRegions = new ITranscriptRegion[5];
            transcriptRegions[0] = new TranscriptRegion(TranscriptRegionType.Exon, 1, 130181729, 130181797, 1, 61);
            transcriptRegions[1] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 130181798, 130182626, 61, 62);
            transcriptRegions[2] = new TranscriptRegion(TranscriptRegionType.Exon, 2, 130182627, 130182775, 62, 210);
            transcriptRegions[3] = new TranscriptRegion(TranscriptRegionType.Intron, 2, 130182776, 130190468, 210, 211);
            transcriptRegions[4] = new TranscriptRegion(TranscriptRegionType.Exon, 3, 130190469, 130190713, 211, 455);

            var codingRegion = new CodingRegion(130181737, 130190626, 1, 368, 369);
            var translation  = new Translation(codingRegion, CompactId.Empty, null);

            _enst00000425361 = new Transcript(_chr2, 130181737, 130190713, CompactId.Convert("ENST00000425361", 5), translation, BioType.other,
                mzt2BGene, 0, 0, false, transcriptRegions, 0, null, 0, 0, Source.Ensembl, false, false, null, null);
            _originTranscripts = new[] {_enst00000425361};

            var transcriptRegions2 = new ITranscriptRegion[10];
            transcriptRegions2[0] = new TranscriptRegion(TranscriptRegionType.Exon, 5, 131464900, 131465047, 532, 679);
            transcriptRegions2[1] = new TranscriptRegion(TranscriptRegionType.Intron, 4, 131465048, 131470205, 531, 532);
            transcriptRegions2[2] = new TranscriptRegion(TranscriptRegionType.Exon, 4, 131470206, 131470343, 394, 531);
            transcriptRegions2[3] = new TranscriptRegion(TranscriptRegionType.Intron, 3, 131470344, 131472067, 393, 394);
            transcriptRegions2[4] = new TranscriptRegion(TranscriptRegionType.Exon, 3, 131472068, 131472182, 279, 393);
            transcriptRegions2[5] = new TranscriptRegion(TranscriptRegionType.Intron, 2, 131472183, 131491875, 278, 279);
            transcriptRegions2[6] = new TranscriptRegion(TranscriptRegionType.Exon, 2, 131491876, 131492024, 130, 278);
            transcriptRegions2[7] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 131492025, 131492206, 129, 130);
            transcriptRegions2[8] = new TranscriptRegion(TranscriptRegionType.Exon, 1, 131492207, 131492335, 1, 129);
            transcriptRegions2[9] = new TranscriptRegion(TranscriptRegionType.Intron, 0, 131492336, 131492341, 0, 0);

            var codingRegion2 = new CodingRegion(131470316, 131492335, 1, 421, 423);
            var translation2  = new Translation(codingRegion2, CompactId.Empty, null);

            _enst00000427024 = new Transcript(_chr2, 131464900, 131492335, CompactId.Convert("ENST00000427024", 5), translation2, BioType.other,
                mzt2AGene, 0, 0, false, transcriptRegions2, 0, null, 0, 0, Source.Ensembl, false, false, null, null);

            var transcriptRegions3 = new ITranscriptRegion[5];
            transcriptRegions3[0] = new TranscriptRegion(TranscriptRegionType.Exon, 3, 131483960, 131484218, 366, 624);
            transcriptRegions3[1] = new TranscriptRegion(TranscriptRegionType.Intron, 2, 131484219, 131491875, 365, 366);
            transcriptRegions3[2] = new TranscriptRegion(TranscriptRegionType.Exon, 2, 131491876, 131492024, 217, 365);
            transcriptRegions3[3] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 131492025, 131492206, 216, 217);
            transcriptRegions3[4] = new TranscriptRegion(TranscriptRegionType.Exon, 1, 131492207, 131492422, 1, 216);

            var codingRegion3 = new CodingRegion(131484061, 131492376, 47, 523, 477);
            var translation3  = new Translation(codingRegion3, CompactId.Empty, null);

            var enst00000309451 = new Transcript(_chr2, 131483960, 131492422, CompactId.Convert("ENST00000309451", 6), translation3, BioType.other,
                mzt2AGene, 0, 0, false, transcriptRegions3, 0, null, 0, 0, Source.Ensembl, false, false, null, null);

            _partnerTranscripts = new[] {_enst00000427024, enst00000309451};
        }

        [Fact]
        public void GetGeneFusionsByTranscript_TrickyCodingRegionLength_OverlappingCodingRegion()
        {
            // chr2 130185834 T [chr2:131488839[
            // ENST00000427024.5, ENST00000425361.5, ENST00000309451.6
            var origin    = new BreakPoint(_chr2, 130185834, true);
            var partner   = new BreakPoint(_chr2, 131488839, false);
            var adjacency = new BreakEndAdjacency(origin, partner);
            
            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion>();
            transcriptIdToGeneFusions.GetGeneFusionsByTranscript(adjacency, _originTranscripts, _partnerTranscripts);

            var observedAnnotatedGeneFusion = transcriptIdToGeneFusions["ENST00000425361.5"];
            Assert.NotNull(observedAnnotatedGeneFusion);

            Assert.Equal(2, observedAnnotatedGeneFusion.Intron);
            Assert.Null(observedAnnotatedGeneFusion.Exon);
            Assert.Equal(2, observedAnnotatedGeneFusion.GeneFusions.Length);

            var fusion = observedAnnotatedGeneFusion.GeneFusions[0];

            Assert.Equal("MZT2A{ENST00000427024.5}:c.1_278+3037_MZT2B{ENST00000425361.5}:c.210+3059_368", fusion.HgvsCoding);
            Assert.Equal(2, fusion.Intron);
            Assert.Null(fusion.Exon);

            var fusion2 = observedAnnotatedGeneFusion.GeneFusions[1];

            Assert.Equal("MZT2A{ENST00000309451.6}:c.1_319+3037_MZT2B{ENST00000425361.5}:c.210+3059_368", fusion2.HgvsCoding);
            Assert.Equal(2, fusion2.Intron);
            Assert.Null(fusion2.Exon);
        }
        
        [Fact]
        public void GetGeneFusionsByTranscript_OverlappingUtr()
        {
            // chr2 130185834 T [chr2:131488839[
            // ENST00000427024.5, ENST00000425361.5, ENST00000309451.6
            var origin    = new BreakPoint(_chr2, 130185834, true); // second
            var partner   = new BreakPoint(_chr2, 131488839, false); // first
            var adjacency = new BreakEndAdjacency(origin, partner);
            
            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion>();
            transcriptIdToGeneFusions.GetGeneFusionsByTranscript(adjacency, _originTranscripts, _partnerTranscripts);

            var observedAnnotatedGeneFusion = transcriptIdToGeneFusions["ENST00000425361.5"];
            Assert.NotNull(observedAnnotatedGeneFusion);

            Assert.Equal(2, observedAnnotatedGeneFusion.Intron);
            Assert.Null(observedAnnotatedGeneFusion.Exon);
            Assert.Equal(2, observedAnnotatedGeneFusion.GeneFusions.Length);

            var fusion = observedAnnotatedGeneFusion.GeneFusions[0];

            Assert.Equal("MZT2A{ENST00000427024.5}:c.1_278+3037_MZT2B{ENST00000425361.5}:c.210+3059_368", fusion.HgvsCoding);
            Assert.Equal(2, fusion.Intron);
            Assert.Null(fusion.Exon);

            var fusion2 = observedAnnotatedGeneFusion.GeneFusions[1];

            Assert.Equal("MZT2A{ENST00000309451.6}:c.1_319+3037_MZT2B{ENST00000425361.5}:c.210+3059_368", fusion2.HgvsCoding);
            Assert.Equal(2, fusion2.Intron);
            Assert.Null(fusion2.Exon);
        }

        [Fact]
        public void GetHgvsCoding_BothInCodingRegion()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131488839, 5);
            var    second         = new BreakPointTranscript(_enst00000425361, 130185834, 3);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);

            Assert.Equal("MZT2A{ENST00000427024.5}:c.1_278+3037_MZT2B{ENST00000425361.5}:c.210+3059_368", observedResult);
        }
        
        [Fact]
        public void GetHgvsCoding_FirstIn5PrimeUtr_SecondInCodingRegion()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131470304, 4);
            var    second         = new BreakPointTranscript(_enst00000425361, 130185834, 3);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);
            
            Assert.Equal("MZT2A{ENST00000427024.5}:c.?_-12_MZT2B{ENST00000425361.5}:c.210+3059_368", observedResult);
        }
        
        [Fact]
        public void GetHgvsCoding_FirstIn3PrimeUtr_SecondInCodingRegion()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131492340, 6);
            var    second         = new BreakPointTranscript(_enst00000425361, 130185834, 3);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);
            
            Assert.Equal("MZT2A{ENST00000427024.5}:c.?_426_MZT2B{ENST00000425361.5}:c.210+3059_368", observedResult);
        }
        
        [Fact]
        public void GetHgvsCoding_FirstInCodingRegion_SecondIn3PrimeUtr()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131488839, 5);
            var    second         = new BreakPointTranscript(_enst00000425361, 130190636, 3);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);
            
            Assert.Equal("MZT2A{ENST00000427024.5}:c.1_278+3037_MZT2B{ENST00000425361.5}:c.378_?", observedResult);
        }
        
        [Fact]
        public void GetHgvsCoding_FirstInCodingRegion_SecondIn5PrimeUtr()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131488839, 5);
            var    second         = new BreakPointTranscript(_enst00000425361, 130181730, 0);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);
            
            Assert.Equal("MZT2A{ENST00000427024.5}:c.1_278+3037_MZT2B{ENST00000425361.5}:c.-7_?", observedResult);
        }
        
        [Fact]
        public void GetHgvsCoding_BothInUtr()
        {
            var    first          = new BreakPointTranscript(_enst00000427024, 131470304, 4);
            var    second         = new BreakPointTranscript(_enst00000425361, 130190636, 3);
            string observedResult = GeneFusionUtilities.GetHgvsCoding(first, second);
            
            Assert.Equal("MZT2A{ENST00000427024.5}:c.?_-12_MZT2B{ENST00000425361.5}:c.378_?", observedResult);
        }
    }
}