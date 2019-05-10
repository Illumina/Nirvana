using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
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

        // NM_033517.1, SHANK3
        private ITranscriptRegion[] _regionsNm33517 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 51113070, 51113132, 1, 63),
            new TranscriptRegion(TranscriptRegionType.Intron, 1, 51113133, 51113475, 63, 64),
            new TranscriptRegion(TranscriptRegionType.Exon, 2, 51113476, 51113679, 64, 267),
            new TranscriptRegion(TranscriptRegionType.Intron, 2, 51113680, 51115049, 267, 268),
            new TranscriptRegion(TranscriptRegionType.Exon, 3, 51115050, 51115121, 268, 339),
            new TranscriptRegion(TranscriptRegionType.Intron, 3, 51115122, 51117012, 339, 340),
            new TranscriptRegion(TranscriptRegionType.Exon, 4, 51117013, 51117121, 340, 448),
            new TranscriptRegion(TranscriptRegionType.Intron, 4, 51117122, 51117196, 448, 449),
            new TranscriptRegion(TranscriptRegionType.Exon, 5, 51117197, 51117348, 449, 600),
            new TranscriptRegion(TranscriptRegionType.Intron, 5, 51117349, 51117446, 600, 601),
            new TranscriptRegion(TranscriptRegionType.Exon, 6, 51117447, 51117614, 601, 768),
            new TranscriptRegion(TranscriptRegionType.Intron, 6, 51117615, 51117739, 768, 769),
            new TranscriptRegion(TranscriptRegionType.Exon, 7, 51117740, 51117856, 769, 885),
            new TranscriptRegion(TranscriptRegionType.Intron, 7, 51117857, 51121767, 885, 886),
            new TranscriptRegion(TranscriptRegionType.Exon, 8, 51121768, 51121845, 886, 963),
            new TranscriptRegion(TranscriptRegionType.Intron, 8, 51121846, 51123012, 963, 964),
            new TranscriptRegion(TranscriptRegionType.Exon, 9, 51123013, 51123079, 964, 1030),
            new TranscriptRegion(TranscriptRegionType.Intron, 9, 51123080, 51133202, 1030, 1031),
            new TranscriptRegion(TranscriptRegionType.Exon, 10, 51133203, 51133474, 1031, 1302),
            new TranscriptRegion(TranscriptRegionType.Intron, 10, 51133475, 51135984, 1302, 1342),
            new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135985, 51135989, 1342, 1346),
            new TranscriptRegion(TranscriptRegionType.Gap, 11, 51135990, 51135991, 1346, 1347),
            new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135992, 51136143, 1347, 1498),
            new TranscriptRegion(TranscriptRegionType.Intron, 11, 51136144, 51137117, 1498, 1499),
            new TranscriptRegion(TranscriptRegionType.Exon, 12, 51137118, 51137231, 1499, 1612),
            new TranscriptRegion(TranscriptRegionType.Intron, 12, 51137232, 51142287, 1612, 1613),
            new TranscriptRegion(TranscriptRegionType.Exon, 13, 51142288, 51142363, 1613, 1688),
            new TranscriptRegion(TranscriptRegionType.Intron, 13, 51142364, 51142593, 1688, 1689),
            new TranscriptRegion(TranscriptRegionType.Exon, 14, 51142594, 51142676, 1689, 1771),
            new TranscriptRegion(TranscriptRegionType.Intron, 14, 51142677, 51143165, 1771, 1772),
            new TranscriptRegion(TranscriptRegionType.Exon, 15, 51143166, 51143290, 1772, 1896),
            new TranscriptRegion(TranscriptRegionType.Intron, 15, 51143291, 51143391, 1896, 1897),
            new TranscriptRegion(TranscriptRegionType.Exon, 16, 51143392, 51143524, 1897, 2029),
            new TranscriptRegion(TranscriptRegionType.Intron, 16, 51143525, 51144499, 2029, 2030),
            new TranscriptRegion(TranscriptRegionType.Exon, 17, 51144500, 51144580, 2030, 2110),
            new TranscriptRegion(TranscriptRegionType.Intron, 17, 51144581, 51150042, 2110, 2111),
            new TranscriptRegion(TranscriptRegionType.Exon, 18, 51150043, 51150066, 2111, 2134),
            new TranscriptRegion(TranscriptRegionType.Intron, 18, 51150067, 51153344, 2134, 2135),
            new TranscriptRegion(TranscriptRegionType.Exon, 19, 51153345, 51153475, 2135, 2265),
            new TranscriptRegion(TranscriptRegionType.Intron, 19, 51153476, 51154096, 2265, 2266),
            new TranscriptRegion(TranscriptRegionType.Exon, 20, 51154097, 51154181, 2266, 2350),
            new TranscriptRegion(TranscriptRegionType.Intron, 20, 51154182, 51158611, 2350, 2351),
            new TranscriptRegion(TranscriptRegionType.Exon, 21, 51158612, 51160865, 2351, 4604),
            new TranscriptRegion(TranscriptRegionType.Intron, 21, 51160866, 51169148, 4604, 4605),
            new TranscriptRegion(TranscriptRegionType.Exon, 22, 51169149, 51171640, 4605, 7096)
        };

        // NM_000682.6
        private readonly ITranscriptRegion[] _regionsNm682 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 96778623, 96780986, 1008, 3371),
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 96780987, 96781984, 1, 998)
        };

        // NM_001317107.1
        private ITranscriptRegion[] _regionsNm1317107 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 22138125, 22138561, 670, 1106),
            new TranscriptRegion(TranscriptRegionType.Gap, 1, 22138562, 22138563, 669, 670),
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 22138564, 22139232, 1, 669)
        };

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
        public void GetCdnaPosition_Snv_AfterOutFrameRnaEditDeletion()
        {
            // NM_001317107.1
            var variant = new Interval(22138550, 22138550);
            var observed = MappedPositionUtilities.GetCdnaPositions(_regionsNm1317107[0], _regionsNm1317107[0], variant, true, false);

            Assert.Equal(681, observed.CdnaStart);
        }

        [Fact]
        public void GetCdnaPosition_Snv_AfterInframeRnaEditInsertion()
        {
            // NM_000682.6
            var variant = new Interval(96780984, 96780984);
            var observed = MappedPositionUtilities.GetCdnaPositions(_regionsNm682[0], _regionsNm682[0], variant, true, false);

            Assert.Equal(1010, observed.CdnaStart);
        }
        //new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135985, 51135989, 1342, 1346),
        //new TranscriptRegion(TranscriptRegionType.Gap, 11, 51135990, 51135991, 1346, 1347),
        //new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135992, 51136143, 1347, 1498),

        [Fact]
        public void GetCdnaPosition_Snv_AfterOutframeRnaEditInsertion()
        {
            // NM_033517.1
            var variant = new Interval(51135986, 51135986);
            var observed = MappedPositionUtilities.GetCdnaPositions(_regionsNm33517[20], _regionsNm33517[20], variant, false, false);

            Assert.Equal(1343, observed.CdnaStart);
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
        public void GetCdsPosition_Snv_AfterOutFrameRnaEditDeletion()
        {
            // NM_001317107.1
            var codingRegion = new CodingRegion(22138201, 22139150, 83, 1030, 948);
            const byte startExonPhase = 0;
            (int cdsStart, _) = MappedPositionUtilities.GetCdsPositions(codingRegion, 681, 681, startExonPhase, false);

            Assert.Equal(599, cdsStart);
        }

        [Fact]
        public void GetCdsPosition_Snv_AfterInframeRnaEditInsertion()
        {
            // NM_000682.6
            var codingRegion = new CodingRegion(96780545, 96781888, 97, 1449, 1344);
            const byte startExonPhase = 0;
            (int cdsStart, _) = MappedPositionUtilities.GetCdsPositions(codingRegion, 1010, 1010, startExonPhase, false);

            Assert.Equal(914, cdsStart);
        }

        [Fact]
        public void GetCdsPosition_Snv_AfterOutframeRnaEditInsertion()
        {
            // NM_033517.1
            var codingRegion = new CodingRegion(51113070, 51169740, 1, 5196, 5157);
            const byte startExonPhase = 0;
            (int cdsStart, _) = MappedPositionUtilities.GetCdsPositions(codingRegion, 1343, 1343, startExonPhase, false);

            Assert.Equal(1343, cdsStart);
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

        private static ITranscriptRegion GetExon() => new TranscriptRegion(TranscriptRegionType.Exon, 0, 10001, 10199, 1, 199);
        private static ITranscriptRegion GetIntron() => new TranscriptRegion(TranscriptRegionType.Intron, 0, 10200, 10299, 199, 200);

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
