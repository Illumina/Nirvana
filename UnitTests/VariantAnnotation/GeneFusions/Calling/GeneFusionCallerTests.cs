using System.Collections.Generic;
using System.Text;
using CacheUtils.TranscriptCache;
using Genome;
using Intervals;
using UnitTests.MockedData;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.GeneFusions.Calling;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Pools;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.Calling
{
    public sealed class GeneFusionCallerTests
    {
        private readonly ITranscript[] _forwardTranscripts          = {Transcripts.ENST00000370673};
        private readonly ITranscript[] _forwardNonCodingTranscripts = {Transcripts.ENST00000427819};
        private readonly ITranscript[] _reverseTranscripts          = {Transcripts.ENST00000615053};

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardFirst5PrimeUtr_ReverseFirstCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298366,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509235, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_-192::ENST00000615053.3(POTEI):r.1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardLast5PrimeUtr_ReverseIntronCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298557,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130508713, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Null(geneFusion.exon);
            Assert.Equal(1,                                                                      geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_-1::ENST00000615053.3(POTEI):r.521+2_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardFirstCds_ReverseFirst3PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298558,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130465652, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(12,                                         geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_1::ENST00000615053.3(POTEI):r.*1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardIntronCds_ReverseLastCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298569,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130465653, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(12,                                         geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_10+2::ENST00000615053.3(POTEI):r.1527_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardLastCds_ReverseLast3PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84349774,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130463799, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(13,                                         geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_351::ENST00000615053.3(POTEI):r.*347_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardFirst3PrimeUtr_ReverseFirst5PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84349775,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509287, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_*1::ENST00000615053.3(POTEI):r.-52_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardLast3PrimeUtr_ReverseLast5PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84350798,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509236, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_*1024::ENST00000615053.3(POTEI):r.-1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseFirst5PrimeUtr_ForwardLastCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130509287, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84349774,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(4,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_351::ENST00000615053.3(POTEI):r.-52_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseLast5PrimeUtr_ForwardFirstCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130509236, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84298558,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_1::ENST00000615053.3(POTEI):r.-1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseFirstCds_ForwardFirst3PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130509235, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84349775,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(4,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_*1::ENST00000615053.3(POTEI):r.1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseLastCds_ForwardLast3PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130465653, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84350798,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(4,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_*1024::ENST00000615053.3(POTEI):r.1527_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseFirst3PrimeUtr_ForwardFirst5PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130465652, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84298366,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_-192::ENST00000615053.3(POTEI):r.*1_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ReverseLast3PrimeUtr_ForwardLast5PrimeUtr_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr2, 130463799, false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr1, 84298557,  true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _reverseTranscripts, _forwardTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000615053.3"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000370673.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_-1::ENST00000615053.3(POTEI):r.*347_?", geneFusion.hgvsr);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardNonCodingFirstCdna_ReverseFirstCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 85276715,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509235, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardNonCodingTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000427819.5"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000427819.5(AL078459.1):r.?_1::ENST00000615053.3(POTEI):r.1_?", geneFusion.hgvsr);
        }
        
        [Fact]
        public void AddGeneFusionsToDictionary_ForwardNonCodingLastCdna_ReverseLastCds_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 85399963,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130465653, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardNonCodingTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000427819.5"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(12,                                         geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000427819.5(AL078459.1):r.?_1950::ENST00000615053.3(POTEI):r.1527_?", geneFusion.hgvsr);
        }
        
        [Fact]
        public void AddGeneFusionsToDictionary_ForwardCds_ReverseCds_InFrame_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298558,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509234, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, false);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_1::ENST00000615053.3(POTEI):r.2_?", geneFusion.hgvsr);
            Assert.True(geneFusion.isInFrame);
        }

        [Fact]
        public void AddGeneFusionsToDictionary_ForwardCds_ReverseCds_Imprecise_NotInFrame_ActualFusion()
        {
            var origin    = new BreakPoint(ChromosomeUtilities.Chr1, 84298558,  false);
            var partner   = new BreakPoint(ChromosomeUtilities.Chr2, 130509234, true);
            var adjacency = new BreakEndAdjacency(origin, partner);

            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();
            GeneFusionCaller.AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, _forwardTranscripts, _reverseTranscripts, true);

            IAnnotatedGeneFusion[] actualGeneFusions = transcriptIdToGeneFusions["ENST00000370673.7"];
            Assert.Single(actualGeneFusions);

            IAnnotatedGeneFusion geneFusion = actualGeneFusions[0];
            Assert.Equal(Transcripts.ENST00000615053.Id.WithVersion, geneFusion.transcript.Id.WithVersion);
            Assert.Equal(1,                                          geneFusion.exon);
            Assert.Null(geneFusion.intron);
            Assert.Equal("ENST00000370673.7(SAMD13):r.?_1::ENST00000615053.3(POTEI):r.2_?", geneFusion.hgvsr);
            Assert.False(geneFusion.isInFrame);
        }

        [Fact]
        public void FoundViableGeneFusion_ReturnTrue()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, false),
                new BreakPoint(ChromosomeUtilities.Chr2, 100, true));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr2, 100, 200);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.Ensembl, Genes.POTEI,
                partnerInterval, Source.Ensembl);
            Assert.True(actualResult);
        }
        
        [Fact]
        public void FoundViableGeneFusion_AffectedByOriginAdjacency_ReturnTrue()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr7,  26241365, true),
                new BreakPoint(ChromosomeUtilities.Chr15, 40854180, false));

            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr7,  26240782, 26252976);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr15, 40820882, 40857210);

            var originGene = new Gene(ChromosomeUtilities.Chr7, 26240782, 26253227, false, "CBX3", 1553, CompactId.Convert("11335"),
                CompactId.Convert("ENSG00000122565"));
            var partnerGene = new Gene(ChromosomeUtilities.Chr15, 40820882, 40857256, true, "CCDC32", 28295, CompactId.Convert("90416"),
                CompactId.Convert("ENSG00000128891"));

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, originGene, originInterval, Source.Ensembl, partnerGene,
                partnerInterval, Source.Ensembl);
            Assert.True(actualResult);
        }

        [Fact]
        public void FoundViableGeneFusion_SameGeneSymbol_ReturnFalse()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, false),
                new BreakPoint(ChromosomeUtilities.Chr2, 100, true));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr2, 100, 200);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.Ensembl, Genes.SAMD13,
                partnerInterval, Source.Ensembl);
            Assert.False(actualResult);
        }
        
        [Fact]
        public void FoundViableGeneFusion_DifferentOriginOrientation_ReturnFalse()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, true),
                new BreakPoint(ChromosomeUtilities.Chr2, 100, true));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr2, 100, 200);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.Ensembl, Genes.POTEI,
                partnerInterval, Source.Ensembl);
            Assert.False(actualResult);
        }

        [Fact]
        public void FoundViableGeneFusion_DifferentPartnerOrientation_ReturnFalse()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, false),
                new BreakPoint(ChromosomeUtilities.Chr2, 100, false));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr2, 100, 200);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.Ensembl, Genes.POTEI,
                partnerInterval, Source.Ensembl);
            Assert.False(actualResult);
        }

        [Fact]
        public void FoundViableGeneFusion_DifferentTranscriptSource_ReturnFalse()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, false),
                new BreakPoint(ChromosomeUtilities.Chr2, 100, true));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr2, 100, 200);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.RefSeq, Genes.POTEI,
                partnerInterval, Source.Ensembl);
            Assert.False(actualResult);
        }

        [Fact]
        public void FoundViableGeneFusion_TranscriptsAlreadyOverlap_ReturnFalse()
        {
            var adjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 100, false),
                new BreakPoint(ChromosomeUtilities.Chr1, 100, true));
            
            var originInterval  = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var partnerInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 105, 205);

            bool actualResult = GeneFusionCaller.FoundViableGeneFusion(adjacency, Genes.SAMD13, originInterval, Source.Ensembl, Genes.POTEI,
                partnerInterval, Source.Ensembl);
            Assert.False(actualResult);
        }

        private sealed class GetCodonPositionData : TheoryData<int, int, byte?>
        {
            public GetCodonPositionData()
            {
                Add(84298557, 0, null); // UTR
                Add(84298558, 0, 1);
                Add(84298559, 0, 2);
                Add(84298560, 0, 3);
                Add(84298561, 0, 1);
                Add(84298562, 0, 2);
                Add(84298563, 0, 3);
                Add(84298568, 1, null); // intron
            }
        }

        [Theory]
        [ClassData(typeof(GetCodonPositionData))]
        public void GetCodonPosition_Forward_ExpectedResults(int genomicPosition, int regionIndex, byte? expectedCodonPosition)
        {
            ITranscript transcript = Transcripts.ENST00000370673;
            byte? actualCodonPosition = GeneFusionCaller.GetCodonPosition(transcript.TranscriptRegions[regionIndex], transcript.Translation,
                transcript.StartExonPhase, transcript.Gene.OnReverseStrand, genomicPosition);
            Assert.Equal(expectedCodonPosition, actualCodonPosition);
        }

        [Theory]
        [InlineData(84298558, 130509234, true)]  // 1 -> 2
        [InlineData(84298559, 130509233, true)]  // 2 -> 3
        [InlineData(84298560, 130509232, true)]  // 3 -> 1
        [InlineData(84298561, 130509231, true)]  // 1 -> 2
        [InlineData(84298562, 130509230, true)]  // 2 -> 3
        [InlineData(84298563, 130509229, true)]  // 3 -> 1
        [InlineData(84298561, 130509227, false)] // 1 -> 3
        [InlineData(84298562, 130509228, false)] // 2 -> 2
        [InlineData(84298563, 130509225, false)] // 3 -> 2
        [InlineData(84298564, 130509226, false)] // 1 -> 1
        [InlineData(84298565, 130509223, false)] // 2 -> 1
        [InlineData(84298566, 130509221, false)] // 3 -> 3
        public void DetermineInFrameFusion_ExpectedResults(int firstGenomicPosition, int secondGenomicPosition, bool expectedResult)
        {
            var  first        = new BreakPointTranscript(Transcripts.ENST00000370673, firstGenomicPosition,  0);
            var  second       = new BreakPointTranscript(Transcripts.ENST00000615053, secondGenomicPosition, 24);
            bool actualResult = GeneFusionCaller.DetermineInFrameFusion(first, second);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetGeneSymbols_SameChromosome()
        {
            IGene a = new Gene(ChromosomeUtilities.Chr1, 1000, 2000, false, "A", 0, CompactId.Empty, CompactId.Empty);
            IGene b = new Gene(ChromosomeUtilities.Chr1, 900,  1900, false, "B", 0, CompactId.Empty, CompactId.Empty);

            var expectedFirstGeneSymbol  = "B";
            var expectedSecondGeneSymbol = "A";

            (ulong _, string actualFirstGeneSymbol, uint _, string actualSecondGeneSymbol, uint _) =
                GeneFusionCaller.GetGeneAndFusionKeys(a, b);

            Assert.Equal(expectedFirstGeneSymbol,  actualFirstGeneSymbol);
            Assert.Equal(expectedSecondGeneSymbol, actualSecondGeneSymbol);
        }

        [Fact]
        public void GetGeneSymbols_DifferentChromosomes()
        {
            IGene a = new Gene(ChromosomeUtilities.Chr1, 1000, 2000, false, "A", 0, CompactId.Empty, CompactId.Empty);
            IGene b = new Gene(ChromosomeUtilities.Chr3, 900,  1900, false, "B", 0, CompactId.Empty, CompactId.Empty);

            var expectedFirstGeneSymbol  = "A";
            var expectedSecondGeneSymbol = "B";
            (ulong _, string actualFirstGeneSymbol, uint _, string actualSecondGeneSymbol, uint _) =
                GeneFusionCaller.GetGeneAndFusionKeys(a, b);

            Assert.Equal(expectedFirstGeneSymbol,  actualFirstGeneSymbol);
            Assert.Equal(expectedSecondGeneSymbol, actualSecondGeneSymbol);
        }

        [Fact]
        public void AddGeneFusions_ExpectedResults()
        {
            const string expectedConsequences = "\"consequence\":[\"unidirectional_gene_fusion\"]";
            const string expectedGeneFusionJson =
                "\"geneFusions\":[{\"transcript\":\"ENST00000615053.3\",\"bioType\":\"protein_coding\",\"exon\":1,\"geneId\":\"ENSG00000196834\",\"hgnc\":\"POTEI\",\"hgvsr\":\"ENST00000370673.7(SAMD13):r.?_1::ENST00000615053.3(POTEI):r.2_?\",\"inFrame\":true}]}";

            IntervalForest<ITranscript> transcriptIntervalForest = GetTranscriptIntervalForest();
            IAnnotatedVariant[]         annotatedVariants        = GetAnnotatedVariants();

            var geneFusionCaller = new GeneFusionCaller(ChromosomeUtilities.RefNameToChromosome, transcriptIntervalForest);
            geneFusionCaller.AddGeneFusions(annotatedVariants, false, false, false);

            IAnnotatedVariant annotatedVariant = annotatedVariants[0];

            var sb = new StringBuilder();
            annotatedVariant.Transcripts[0].SerializeJson(sb);
            var json = sb.ToString();
            
            VariantPool.Return((Variant)annotatedVariant.Variant);
            AnnotatedTranscriptPool.Return((AnnotatedTranscript) annotatedVariant.Transcripts[0]);
            AnnotatedVariantPool.Return((AnnotatedVariant)annotatedVariant);

            Assert.Contains(expectedConsequences,   json);
            Assert.Contains(expectedGeneFusionJson, json);
        }

        private IAnnotatedVariant[] GetAnnotatedVariants()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 84298558, 84298558, "A", "A]chr2:130509234]", VariantType.translocation_breakend,
                "1-84298558-A-A]chr2:130509234]", false, false, false, null, AnnotationBehavior.StructuralVariants, true);

            var annotatedTranscript = AnnotatedTranscriptPool.Get(Transcripts.ENST00000370673, null, null, null, null, null, null, null, null, null,
                new List<ConsequenceTag>(), null);
            
            var annotatedVariant = AnnotatedVariantPool.Get(variant);
            annotatedVariant.Transcripts.Add(annotatedTranscript);
            
            return new IAnnotatedVariant[] {annotatedVariant};
        }

        private IntervalForest<ITranscript> GetTranscriptIntervalForest()
        {
            var transcripts = new List<ITranscript>();
            transcripts.AddRange(_forwardTranscripts);
            transcripts.AddRange(_reverseTranscripts);

            IntervalArray<ITranscript>[] intervalArrays = transcripts.ToIntervalArrays(2);
            return new IntervalForest<ITranscript>(intervalArrays);
        }
    }
}