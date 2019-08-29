using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class MappedPositionUtilitiesTests
    {
        private readonly ITranscriptRegion[] _forwardTranscriptRegions = new ITranscriptRegion[]
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1,   77997792, 77998025, 1, 234),
            new TranscriptRegion(TranscriptRegionType.Intron, 1, 77998026, 78001531, 234, 235),
            new TranscriptRegion(TranscriptRegionType.Exon, 2,   78001532, 78001723, 235, 426),
            new TranscriptRegion(TranscriptRegionType.Intron, 2, 78001724, 78024286, 426, 427),
            new TranscriptRegion(TranscriptRegionType.Exon, 3,   78024287, 78024416, 427, 556)
        };

        // ENST00000591244
        private readonly ITranscriptRegion[] _reverseTranscriptRegions = new ITranscriptRegion[]
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

        private const int ForwardVariantStart = 78024346;
        private const int ForwardVariantEnd   = 78024345;

        // Mother.vcf: chr2    313885  .       CTGATTTGCTATGAAA        C
        private const int ReverseVariantStart = 313886;
        private const int ReverseVariantEnd   = 313900;

        // NM_033517.1, SHANK3
        private readonly ITranscriptRegion[] _regionsNm33517 =
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

        private readonly IRnaEdit[] _editsNm33517 =
        {
            new RnaEdit(1303, 1302, "AGCCCGAGCGGGCCCGGCGGCCCCGGCCCCGCGCCCGGC"),
            new RnaEdit(1304, 1304, "C"),
            new RnaEdit(1308, 1309, ""),
            new RnaEdit(7060, 7059, "AAAAAAAAAAAAAAAAA")
        };

        // NM_000682.6
        private readonly ITranscriptRegion[] _regionsNm682 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 96778623, 96780986, 1008, 3371),
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 96780987, 96781984, 1, 998)
        };

        private readonly IRnaEdit[] _editsNm682 =
        {
            new RnaEdit(999, 998, "AGAGGAGGA")
        };

        // NM_001317107.1
        private readonly ITranscriptRegion[] _regionsNm1317107 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 22138125, 22138561, 670, 1106),
            new TranscriptRegion(TranscriptRegionType.Gap, 1, 22138562, 22138563, 669, 670),
            new TranscriptRegion(TranscriptRegionType.Exon, 1, 22138564, 22139232, 1, 669)
        };

        private readonly IRnaEdit[] _editsNm1317107 =
        {
            new RnaEdit(905, 905, "T"),
            new RnaEdit(796, 796, "C"),
            new RnaEdit(679, 679, "A"),
            new RnaEdit(670, 671, "")
        };

        private readonly ITranscriptRegion _exon   = new TranscriptRegion(TranscriptRegionType.Exon, 0, 10001, 10199, 1, 199);
        private readonly ITranscriptRegion _intron = new TranscriptRegion(TranscriptRegionType.Intron, 0, 10200, 10299, 199, 200);

        [Fact]
        public void FindRegion_Forward_Insertion()
        {
            (int startIndex, ITranscriptRegion startRegion) = MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantStart);
            (int endIndex, ITranscriptRegion endRegion)     = MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantEnd);

            Assert.Equal(4, startIndex);
            Assert.Equal(4, endIndex);
            Assert.NotNull(startRegion);
            Assert.NotNull(endRegion);
        }

        [Fact]
        public void FindRegion_Reverse_Deletion()
        {
            (int startIndex, ITranscriptRegion startRegion) = MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantStart);
            (int endIndex, ITranscriptRegion endRegion) = MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantEnd);

            Assert.Equal(6, startIndex);
            Assert.Equal(7, endIndex);
            Assert.NotNull(startRegion);
            Assert.NotNull(endRegion);
        }

        [Fact]
        public void GetCdnaPosition_Forward_Insertion()
        {
            var variant  = new Interval(ForwardVariantStart, ForwardVariantEnd);
            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetCdnaPositions(_forwardTranscriptRegions[4],
                _forwardTranscriptRegions[4], variant, false, true);

            Assert.Equal(486, cdnaStart);
            Assert.Equal(485, cdnaEnd);
        }

        [Fact]
        public void GetCdnaPosition_Reverse_Deletion()
        {
            var variant  = new Interval(ReverseVariantStart, ReverseVariantEnd);
            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetCdnaPositions(_reverseTranscriptRegions[6], _reverseTranscriptRegions[7], variant, true, false);

            Assert.Equal(123, cdnaStart);
            Assert.Equal(-1, cdnaEnd);
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
            (int start, int end) = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, -1, 300, 2, false);
            Assert.Equal(1, start);
            Assert.Equal(300, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartIntron_EndExon()
        {
            (int start, int end) = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, 1, 300, 2, false);
            Assert.Equal(235, start);
            Assert.Equal(300, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartExon_EndIntron()
        {
            (int start, int end) = _forwardTranscriptRegions.GetCoveredCdnaPositions(400, 2, -1, 3, false);
            Assert.Equal(400, start);
            Assert.Equal(426, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartExon_EndAfter()
        {
            (int start, int end) = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, ~_forwardTranscriptRegions.Length, 300, 2, false);
            Assert.Equal(300, start);
            Assert.Equal(556, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartBefore_EndAfter()
        {
            (int start, int end) = _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_forwardTranscriptRegions.Length, false);
            Assert.Equal(1, start);
            Assert.Equal(556, end);
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

            (int start, int end) = regions.GetCoveredCdnaPositions(-1, -12, 11666, 1, false);
            Assert.Equal(11666, start);
            Assert.Equal(11666, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndExon()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 103288513, 103288696, 522, 705)
            };

            // ClinVar ENST00000546844 103288512
            (int start, int end) = regions.GetCoveredCdnaPositions(523, -1, -1, 0, true);
            Assert.Equal(523, start);
            Assert.Equal(705, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartIntron_EndExon()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 103271329, 103288512, 825, 826),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 103288513, 103288696, 642, 825)
            };

            // ENST00000553106
            // 12-103288511-ACCTGTGTCTTTCTTCTTATCTCGTGAAAGCTCATGGACAGTGGCACCAATGTCATGCCTCAAGATCTTGATGATGTTTGTCAGAGCAGGCAGGCTACGTTTATCCAAATGGGTGAAAAATTCATACTCATCTTTCTTTAAACGAGAAGGTCTAGATTCAATGTGGGTCAGGTTTACATCATTCT-A
            (int start, int end) = regions.GetCoveredCdnaPositions(643, 0, -1, 1, true);

            Assert.Equal(643, start);
            Assert.Equal(825, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartExon_EndIntron()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   2, 25398208, 25398329, 167, 288),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 25398330, 25403697, 166, 167),
            };

            // ENST00000556131
            // 12-25398328-GCCTTATA-G
            (int start, int end) = regions.GetCoveredCdnaPositions(-1, 0, 167, 1, true);
            Assert.Equal(167, start);
            Assert.Equal(167, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartExon_EndAfter()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 103288513, 103288696, 1, 825)
            };

            // synthetic
            (int start, int end) = regions.GetCoveredCdnaPositions(-1, ~1, -1, -1, true);
            Assert.Equal(1, start);
            Assert.Equal(825, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndAfter()
        {
            (int start, int end) = _reverseTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_reverseTranscriptRegions.Length, true);
            Assert.Equal(1, start);
            Assert.Equal(811, end);
        }

        [Fact]
        public void GetCdsPosition_Forward_Insertion()
        {
            var codingRegion = new CodingRegion(78001559, 78024355, 262, 495, 234);
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 486, 485, startExonPhase, true);

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

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 29, 28, startExonPhase, true);

            Assert.Equal(30, cdsStart);
            Assert.Equal(29, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_Reverse_NoCodingRegion_Deletion()
        {
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(null, -1, 123, startExonPhase, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Forward()
        {
            // variant: [6647337, 6647336] insertion after coding region
            var codingRegion = new CodingRegion(6643999, 6647336, 667, 1674, 1008);
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 1675, 1674, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Reverse()
        {
            // variant: [103629803, 103629804] insertion after coding region
            var codingRegion = new CodingRegion(103113259, 103629803, 161, 10543, 10383);
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 161, 160, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionBeforeCodingRegion_Reverse()
        {
            // variant: [37480320, 37480319] insertion after coding region
            var codingRegion = new CodingRegion(37480320, 37543667, 556, 3228, 2673);
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 3229, 3228, startExonPhase, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_DoNotSilenceOutput_Reverse()
        {
            // variant: [179315139, 179315692]
            var codingRegion = new CodingRegion(179308070, 179315170, 617, 942, 326);
            const byte startExonPhase = 0;

            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(codingRegion, 95, 648, startExonPhase, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(32, cdsEnd);
        }

        [Fact]
        public void GetProteinPosition_Forward_Insertion()
        {
            int proteinStart = MappedPositionUtilities.GetProteinPosition(225);
            int proteinEnd   = MappedPositionUtilities.GetProteinPosition(224);
            Assert.Equal(75, proteinStart);
            Assert.Equal(75, proteinEnd);
        }

        [Fact]
        public void GetProteinPosition_Reverse_Deletion()
        {
            int proteinStart = MappedPositionUtilities.GetProteinPosition(-1);
            int proteinEnd   = MappedPositionUtilities.GetProteinPosition(-1);
            Assert.Equal(-1, proteinStart);
            Assert.Equal(-1, proteinEnd);
        }

        [Fact]
        public void FoundExonEndpointInsertion_NotInsertion_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(false, -1, 100, _exon, _intron));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothExons_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, 100, _exon, _exon));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothIntrons_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, 100, _intron, _intron));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothDefinedCdnaPositions_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, 100, 110, _exon, _intron));
        }

        [Fact]
        public void FoundExonEndpointInsertion_BothUndefinedCdnaPositions_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, -1, _exon, _intron));
        }

        [Fact]
        public void FoundExonEndpointInsertion_UndefinedRegion_ReturnFalse()
        {
            Assert.False(MappedPositionUtilities.FoundExonEndpointInsertion(true, -1, -1, null, _intron));
        }

        [Fact]
        public void FoundExonEndpointInsertion_OneIntron_OneExon_OneUndefinedPosition_ReturnTrue()
        {
            Assert.True(MappedPositionUtilities.FoundExonEndpointInsertion(true, 108, -1, _exon, _intron));
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantEnd_ExonEnd_Reverse()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Intron, 7, 243736351, 243776972, 762, 763);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Exon, 8, 243736228, 243736350, 763, 885);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.FixExonEndpointInsertion(-1, 763, true, startRegion, endRegion,
                new Interval(243736351, 243736350));

            Assert.Equal(762, cdnaStart);
            Assert.Equal(763, cdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantStart_ExonStart_Reverse()
        {
            // N.B. this configuration has never been spotted in the wild
            var startRegion = new TranscriptRegion(TranscriptRegionType.Exon, 2, 2000, 2199, 1, 200);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Intron, 2, 1999, 1000, 200, 201);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.FixExonEndpointInsertion(200, -1, true, startRegion, endRegion,
                new Interval(2000, 1999));

            Assert.Equal(200, cdnaStart);
            Assert.Equal(201, cdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantEnd_ExonEnd_Forward()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Intron, 16, 89521770, 89528546, 3071, 3072);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Exon,   16, 89521614, 89521769, 2916, 3071);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.FixExonEndpointInsertion(-1, 3071, false, startRegion, endRegion,
                new Interval(89521770, 89521769));

            Assert.Equal(3072, cdnaStart);
            Assert.Equal(3071, cdnaEnd);
        }

        [Fact]
        public void FixExonEndpointInsertion_VariantStart_ExonStart_Forward()
        {
            var startRegion = new TranscriptRegion(TranscriptRegionType.Exon,   2, 99459243, 99459360, 108, 225);
            var endRegion   = new TranscriptRegion(TranscriptRegionType.Intron, 1, 99456512, 99459242, 107, 108);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.FixExonEndpointInsertion(108, -1, false, startRegion, endRegion,
                new Interval(99459243, 99459242));

            Assert.Equal(108, cdnaStart);
            Assert.Equal(107, cdnaEnd);
        }

        [Theory]
        [InlineData(102, 105, 0, 101, 200, 2, 5, 1, 2)]
        [InlineData(102, 105, 2, 101, 200, 4, 7, 2, 3)]
        [InlineData(94, 130, 0, 101, 200, 1, 30, 1, 10)]
        [InlineData(94, 130, 2, 101, 200, 3, 32, 1, 11)]
        [InlineData(101, 130, 0, 101, 200, 1, 30, 1, 10)]
        [InlineData(199, 204, 0, 101, 200, 99, 100, 33, 34)]
        [InlineData(199, 204, 2, 101, 200, 101, 102, 34, 34)]
        [InlineData(1, 300, 0, 101, 200, 1, 100, 1, 34)]
        [InlineData(1, 300, 2, 101, 200, 3, 102, 1, 34)]
        public void GetCoveredCdsAndProteinPositions_AsExpected(int coveredCdnaStart, int coveredCdnaEnd, byte startExonPhase, int codingCdnaStart, int codingCdnaEnd, int expectedCdsStart, int expectedCdsEnd, int expectedProteinStart, int expectedProteinEnd)
        {
            var codingRegion = new CodingRegion(-1, -1, codingCdnaStart, codingCdnaEnd, codingCdnaEnd - codingCdnaStart + 1);
            var coveredPositions= MappedPositionUtilities.GetCoveredCdsAndProteinPositions(coveredCdnaStart, coveredCdnaEnd, startExonPhase, codingRegion);
            Assert.Equal(expectedCdsStart, coveredPositions.CdsStart);
            Assert.Equal(expectedCdsEnd, coveredPositions.CdsEnd);
            Assert.Equal(expectedProteinStart, coveredPositions.ProteinStart);
            Assert.Equal(expectedProteinEnd, coveredPositions.ProteinEnd);
        }
    }
}
