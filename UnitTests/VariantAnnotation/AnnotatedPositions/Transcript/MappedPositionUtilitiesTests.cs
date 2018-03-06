using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class MappedPositionUtilitiesTests
    {
        private readonly ITranscriptRegion[] _forwardTranscriptRegions;
        private readonly ITranscriptRegion[] _reverseTranscriptRegions;

        private const int ForwardVariantStart = 78024346;
        private const int ForwardVariantEnd   = 78024345;

        // Mother.vcf: chr2    313885  .       CTGATTTGCTATGAAA        C
        private const int ReverseVariantStart = 313886;
        private const int ReverseVariantEnd   = 313900;

        public MappedPositionUtilitiesTests()
        {
            _forwardTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1,   77997792, 77998025, 1, 234),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 77998026, 78001531, 234, 235),
                new TranscriptRegion(TranscriptRegionType.Exon, 2,   78001532, 78001723, 235, 426),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 78001724, 78024286, 426, 427),
                new TranscriptRegion(TranscriptRegionType.Exon, 3,   78024287, 78024416, 427, 556)
            };

            // ENST00000591244
            _reverseTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 5,   309218, 309407, 622, 811),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 309408, 310214, 621, 622),
                new TranscriptRegion(TranscriptRegionType.Exon, 4,   310215, 310499, 337, 621),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 310500, 312956, 336, 337),
                new TranscriptRegion(TranscriptRegionType.Exon, 3,   312957, 313157, 136, 336),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 313158, 313873, 135, 136),
                new TranscriptRegion(TranscriptRegionType.Exon, 2,   313874, 313892, 117, 135),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 313893, 314242, 116, 117),
                new TranscriptRegion(TranscriptRegionType.Exon, 1,   314243, 314358, 1, 116)
            };
        }

        [Fact]
        public void FindRegion_Forward_Insertion()
        {
            var observedStart = MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantStart);
            var observedEnd   = MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantEnd);

            Assert.Equal(4, observedStart.Index);
            Assert.Equal(4, observedEnd.Index);
            Assert.NotNull(observedStart.Region);
            Assert.NotNull(observedEnd.Region);
        }

        [Fact]
        public void FindRegion_Reverse_Deletion()
        {
            var observedStart = MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantStart);
            var observedEnd   = MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantEnd);

            Assert.Equal(6, observedStart.Index);
            Assert.Equal(7, observedEnd.Index);
            Assert.NotNull(observedStart.Region);
            Assert.NotNull(observedEnd.Region);
        }

        [Fact]
        public void GetCdnaPosition_Forward_Insertion()
        {
            var variant  = new Interval(ForwardVariantStart, ForwardVariantEnd);
            var observed = MappedPositionUtilities.GetCdnaPositions(_forwardTranscriptRegions[4],
                _forwardTranscriptRegions[4], variant, false, true);

            Assert.Equal(486, observed.CdnaStart);
            Assert.Equal(485, observed.CdnaEnd);
        }

        [Fact]
        public void GetCdnaPosition_Reverse_Deletion()
        {
            var variant  = new Interval(ReverseVariantStart, ReverseVariantEnd);
            var observed = MappedPositionUtilities.GetCdnaPositions(_reverseTranscriptRegions[6], _reverseTranscriptRegions[7], variant, true, false);

            Assert.Equal(123, observed.CdnaStart);
            Assert.Equal(-1, observed.CdnaEnd);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartBefore_EndExon()
        {
            var observedResults = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, -1, 300, 2, false);
            Assert.Equal(1, observedResults.Start);
            Assert.Equal(300, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartIntron_EndExon()
        {
            var observedResults = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, 1, 300, 2, false);
            Assert.Equal(235, observedResults.Start);
            Assert.Equal(300, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartExon_EndAfter()
        {
            var observedResults = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, ~_forwardTranscriptRegions.Length, 300, 2, false);
            Assert.Equal(300, observedResults.Start);
            Assert.Equal(556, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartBefore_EndAfter()
        {
            var observedResults = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_forwardTranscriptRegions.Length, false);
            Assert.Equal(1, observedResults.Start);
            Assert.Equal(556, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_Insertion_StartAfter_EndExon()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 10, 19, 10, 19),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 20, 29, 19, 20),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 30, 39, 20, 29),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 40, 49, 29, 30),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 50, 59, 30, 39),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 60, 69, 39, 40),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 70, 79, 40, 49),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 80, 89, 49, 50),
                new TranscriptRegion(TranscriptRegionType.Exon, 5, 90, 4834618, 50, 1676),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 4834619, 4842604, 1676, 1677),
                new TranscriptRegion(TranscriptRegionType.Exon, 6, 4842605, 4852594, 1677, 11666)
            };

            var observedResults = regions.GetCoveredCdnaPositions(-1, -12, 11666, 1, false);
            Assert.Equal(11666, observedResults.Start);
            Assert.Equal(11666, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndExon()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 103288513, 103288696, 522, 705)
            };

            // ClinVar ENST00000546844 103288512
            var observedResults = regions.GetCoveredCdnaPositions(523, -1, -1, 0, true);
            Assert.Equal(523, observedResults.Start);
            Assert.Equal(705, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartIntron_EndExon()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 103271329, 103288512, 825, 826),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 103288513, 103288696, 642, 825)
            };

            // ClinVar ENST00000553106 103288512
            var observedResults = regions.GetCoveredCdnaPositions(643, 0, -1, 1, true);
            Assert.Equal(643, observedResults.Start);
            Assert.Equal(825, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartExon_EndAfter()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 103288513, 103288696, 1, 825)
            };

            // synthetic
            var observedResults = regions.GetCoveredCdnaPositions(-1, ~1, -1, -1, true);
            Assert.Equal(1, observedResults.Start);
            Assert.Equal(825, observedResults.End);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndAfter()
        {
            var observedResults = _reverseTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_reverseTranscriptRegions.Length, true);
            Assert.Equal(1, observedResults.Start);
            Assert.Equal(811, observedResults.End);
        }

        [Fact]
        public void GetCdsPosition_Forward_Insertion()
        {
            var codingRegion = new CodingRegion(78001559, 78024355, 262, 495, 234);
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 486, 485, startExonPhase, true);

            Assert.Equal(225, cdsStart);
            Assert.Equal(224, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_Forward_Insertion_WithStartExonPhase()
        {
            var codingRegion = new CodingRegion(6413107, 6415837, 1, 953, 953);
            const byte startExonPhase = 1;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 29, 28, startExonPhase, true);

            Assert.Equal(30, cdsStart);
            Assert.Equal(29, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_Reverse_NoCodingRegion_Deletion()
        {
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(null, -1, 123, startExonPhase, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Forward()
        {
            // variant: [6647337, 6647336] insertion after coding region
            var codingRegion = new CodingRegion(6643999, 6647336, 667, 1674, 1008);
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 1675, 1674, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Reverse()
        {
            // variant: [103629803, 103629804] insertion after coding region
            var codingRegion = new CodingRegion(103113259, 103629803, 161, 10543, 10383);
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 161, 160, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionBeforeCodingRegion_Reverse()
        {
            // variant: [37480320, 37480319] insertion after coding region
            var codingRegion = new CodingRegion(37480320, 37543667, 556, 3228, 2673);
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 3229, 3228, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_DoNotSilenceOutput_Reverse()
        {
            // variant: [179315139, 179315692]
            var codingRegion = new CodingRegion(179308070, 179315170, 617, 942, 326);
            const byte startExonPhase = 0;

            var (cdsStart, cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 95, 648, startExonPhase, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(32, cdsEnd);
        }

        [Fact]
        public void GetProteinPosition_Forward_Insertion()
        {
            var proteinStart = MappedPositionUtilities.GetProteinPosition(225);
            var proteinEnd   = MappedPositionUtilities.GetProteinPosition(224);
            Assert.Equal(75, proteinStart);
            Assert.Equal(75, proteinEnd);
        }

        [Fact]
        public void GetProteinPosition_Reverse_Deletion()
        {
            var proteinStart = MappedPositionUtilities.GetProteinPosition(-1);
            var proteinEnd   = MappedPositionUtilities.GetProteinPosition(-1);
            Assert.Equal(-1, proteinStart);
            Assert.Equal(-1, proteinEnd);
        }

        private ITranscriptRegion GetExon() => new TranscriptRegion(TranscriptRegionType.Exon, 0, 10001, 10199, 1, 199);
        private ITranscriptRegion GetIntron() => new TranscriptRegion(TranscriptRegionType.Intron, 0, 10200, 10299, 199, 200);

        [Fact]
        public void FoundExonEndpointInsertion_NotInsertion_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(false, -1, 100, GetExon(), GetIntron()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothExons_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, 100, GetExon(), GetExon()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothIntrons_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, 100, GetIntron(), GetIntron()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothDefinedCdnaPositions_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, 100, 110, GetExon(), GetIntron()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothUndefinedCdnaPositions_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, -1, GetExon(), GetIntron()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_UndefinedRegion_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, -1, null, GetIntron()));
        }

        [Fact]
        public void FoundExonEndpointInsertion_OneIntron_OneExon_OneUndefinedPosition_ReturnTrue()
        {
            Assert.True(MappedPositionUtilities.FoundExonEndpointInsertion(true, 108, -1, GetExon(), GetIntron()));
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantEnd_ExonEnd_Reverse()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Intron, 7, 243736351, 243776972, 762, 763);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Exon, 8, 243736228, 243736350, 763, 885);

            var result = MappedPositionUtilities.FixExonEndpointInsertion(-1, 763, true, startRegion, endRegion,
                new Interval(243736351, 243736350));

            Assert.Equal(762, result.CdnaStart);
            Assert.Equal(763, result.CdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantStart_ExonStart_Reverse()
        {
            // N.B. this configuration has never been spotted in the wild
            var startRegion = new TranscriptRegion(TranscriptRegionType.Exon, 2, 2000, 2199, 1, 200);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Intron, 2, 1999, 1000, 200, 201);

            var result = MappedPositionUtilities.FixExonEndpointInsertion(200, -1, true, startRegion, endRegion,
                new Interval(2000, 1999));

            Assert.Equal(200, result.CdnaStart);
            Assert.Equal(201, result.CdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantEnd_ExonEnd_Forward()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Intron, 16, 89521770, 89528546, 3071, 3072);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Exon,   16, 89521614, 89521769, 2916, 3071);

            var result = MappedPositionUtilities.FixExonEndpointInsertion(-1, 3071, false, startRegion, endRegion,
                new Interval(89521770, 89521769));

            Assert.Equal(3072, result.CdnaStart);
            Assert.Equal(3071, result.CdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantStart_ExonStart_Forward()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Exon,   2, 99459243, 99459360, 108, 225);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Intron, 1, 99456512, 99459242, 107, 108);

            var result = MappedPositionUtilities.FixExonEndpointInsertion(108, -1, false, startRegion, endRegion,
                new Interval(99459243, 99459242));

            Assert.Equal(108, result.CdnaStart);
            Assert.Equal(107, result.CdnaEnd);
        }
    }
}
