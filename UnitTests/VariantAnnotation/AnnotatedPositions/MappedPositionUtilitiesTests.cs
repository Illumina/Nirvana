using Cache.Data;
using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class MappedPositionUtilitiesTests
    {
        private readonly TranscriptRegion[] _forwardTranscriptRegions =
        {
            new(77997792, 77998025, 1, 234, TranscriptRegionType.Exon, 1, null),
            new(77998026, 78001531, 234, 235, TranscriptRegionType.Intron, 1, null),
            new(78001532, 78001723, 235, 426, TranscriptRegionType.Exon, 2, null),
            new(78001724, 78024286, 426, 427, TranscriptRegionType.Intron, 2, null),
            new(78024287, 78024416, 427, 556, TranscriptRegionType.Exon, 3, null)
        };

        // ENST00000591244
        private readonly TranscriptRegion[] _reverseTranscriptRegions =
        {
            new(309218, 309407, 622, 811, TranscriptRegionType.Exon, 5, null),
            new(309408, 310214, 621, 622, TranscriptRegionType.Intron, 4, null),
            new(310215, 310499, 337, 621, TranscriptRegionType.Exon, 4, null),
            new(310500, 312956, 336, 337, TranscriptRegionType.Intron, 3, null),
            new(312957, 313157, 136, 336, TranscriptRegionType.Exon, 3, null),
            new(313158, 313873, 135, 136, TranscriptRegionType.Intron, 2, null),
            new(313874, 313892, 117, 135, TranscriptRegionType.Exon, 2, null),
            new(313893, 314242, 116, 117, TranscriptRegionType.Intron, 1, null),
            new(314243, 314358, 1, 116, TranscriptRegionType.Exon, 1, null)
        };

        private const int ForwardVariantStart = 78024346;
        private const int ForwardVariantEnd   = 78024345;

        // Mother.vcf: chr2    313885  .       CTGATTTGCTATGAAA        C
        private const int ReverseVariantStart = 313886;
        private const int ReverseVariantEnd   = 313900;

        // NM_033517.1, SHANK3
        private readonly TranscriptRegion[] _regionsNm33517 =
        {
            new(51113070, 51113132, 1, 63, TranscriptRegionType.Exon, 1, null),
            new(51113133, 51113475, 63, 64, TranscriptRegionType.Intron, 1, null),
            new(51113476, 51113679, 64, 267, TranscriptRegionType.Exon, 2, null),
            new(51113680, 51115049, 267, 268, TranscriptRegionType.Intron, 2, null),
            new(51115050, 51115121, 268, 339, TranscriptRegionType.Exon, 3, null),
            new(51115122, 51117012, 339, 340, TranscriptRegionType.Intron, 3, null),
            new(51117013, 51117121, 340, 448, TranscriptRegionType.Exon, 4, null),
            new(51117122, 51117196, 448, 449, TranscriptRegionType.Intron, 4, null),
            new(51117197, 51117348, 449, 600, TranscriptRegionType.Exon, 5, null),
            new(51117349, 51117446, 600, 601, TranscriptRegionType.Intron, 5, null),
            new(51117447, 51117614, 601, 768, TranscriptRegionType.Exon, 6, null),
            new(51117615, 51117739, 768, 769, TranscriptRegionType.Intron, 6, null),
            new(51117740, 51117856, 769, 885, TranscriptRegionType.Exon, 7, null),
            new(51117857, 51121767, 885, 886, TranscriptRegionType.Intron, 7, null),
            new(51121768, 51121845, 886, 963, TranscriptRegionType.Exon, 8, null),
            new(51121846, 51123012, 963, 964, TranscriptRegionType.Intron, 8, null),
            new(51123013, 51123079, 964, 1030, TranscriptRegionType.Exon, 9, null),
            new(51123080, 51133202, 1030, 1031, TranscriptRegionType.Intron, 9, null),
            new(51133203, 51133474, 1031, 1302, TranscriptRegionType.Exon, 10, null),
            new(51133475, 51135984, 1302, 1342, TranscriptRegionType.Intron, 10, null),
            new(51135985, 51136143, 1342, 1498, TranscriptRegionType.Exon, 11, null),
            new(51136144, 51137117, 1498, 1499, TranscriptRegionType.Intron, 11, null),
            new(51137118, 51137231, 1499, 1612, TranscriptRegionType.Exon, 12, null),
            new(51137232, 51142287, 1612, 1613, TranscriptRegionType.Intron, 12, null),
            new(51142288, 51142363, 1613, 1688, TranscriptRegionType.Exon, 13, null),
            new(51142364, 51142593, 1688, 1689, TranscriptRegionType.Intron, 13, null),
            new(51142594, 51142676, 1689, 1771, TranscriptRegionType.Exon, 14, null),
            new(51142677, 51143165, 1771, 1772, TranscriptRegionType.Intron, 14, null),
            new(51143166, 51143290, 1772, 1896, TranscriptRegionType.Exon, 15, null),
            new(51143291, 51143391, 1896, 1897, TranscriptRegionType.Intron, 15, null),
            new(51143392, 51143524, 1897, 2029, TranscriptRegionType.Exon, 16, null),
            new(51143525, 51144499, 2029, 2030, TranscriptRegionType.Intron, 16, null),
            new(51144500, 51144580, 2030, 2110, TranscriptRegionType.Exon, 17, null),
            new(51144581, 51150042, 2110, 2111, TranscriptRegionType.Intron, 17, null),
            new(51150043, 51150066, 2111, 2134, TranscriptRegionType.Exon, 18, null),
            new(51150067, 51153344, 2134, 2135, TranscriptRegionType.Intron, 18, null),
            new(51153345, 51153475, 2135, 2265, TranscriptRegionType.Exon, 19, null),
            new(51153476, 51154096, 2265, 2266, TranscriptRegionType.Intron, 19, null),
            new(51154097, 51154181, 2266, 2350, TranscriptRegionType.Exon, 20, null),
            new(51154182, 51158611, 2350, 2351, TranscriptRegionType.Intron, 20, null),
            new(51158612, 51160865, 2351, 4604, TranscriptRegionType.Exon, 21, null),
            new(51160866, 51169148, 4604, 4605, TranscriptRegionType.Intron, 21, null),
            new(51169149, 51171640, 4605, 7096, TranscriptRegionType.Exon, 22, null)
        };

        // NM_000682.6
        private readonly TranscriptRegion[] _regionsNm682 =
        {
            new(96778623, 96780986, 1008, 3371, TranscriptRegionType.Exon, 1, null),
            new(96780987, 96781984, 1, 998, TranscriptRegionType.Exon, 1, null)
        };

        // NM_001317107.1
        private readonly TranscriptRegion[] _regionsNm1317107 =
        {
            new(22138125, 22139232, 1, 1106, TranscriptRegionType.Exon, 1,
                new CigarOp[]
                {
                    new(CigarType.Match, 669),
                    new(CigarType.Deletion, 2),
                    new(CigarType.Match, 437)
                })
        };

        [Fact]
        public void FindRegion_Forward_Insertion()
        {
            (int startIndex, TranscriptRegion startRegion) =
                MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantStart);
            (int endIndex, TranscriptRegion endRegion) =
                MappedPositionUtilities.FindRegion(_forwardTranscriptRegions, ForwardVariantEnd);

            Assert.Equal(4, startIndex);
            Assert.Equal(4, endIndex);
            Assert.NotNull(startRegion);
            Assert.NotNull(endRegion);
        }

        [Fact]
        public void FindRegion_Reverse_Deletion()
        {
            (int startIndex, TranscriptRegion startRegion) =
                MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantStart);
            (int endIndex, TranscriptRegion endRegion) =
                MappedPositionUtilities.FindRegion(_reverseTranscriptRegions, ReverseVariantEnd);

            Assert.Equal(6, startIndex);
            Assert.Equal(7, endIndex);
            Assert.NotNull(startRegion);
            Assert.NotNull(endRegion);
        }

        [Fact]
        public void GetCdnaPosition_Forward_Insertion()
        {
            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetInsertionCdnaPositions(
                _forwardTranscriptRegions[4], _forwardTranscriptRegions[4], ForwardVariantStart, ForwardVariantEnd,
                false);

            Assert.Equal(486, cdnaStart);
            Assert.Equal(485, cdnaEnd);
        }

        [Fact]
        public void GetCdnaPosition_Reverse_Deletion()
        {
            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetCdnaPositions(_reverseTranscriptRegions[6],
                _reverseTranscriptRegions[7], ReverseVariantStart, ReverseVariantEnd, true);

            Assert.Equal(123, cdnaStart);
            Assert.Equal(-1, cdnaEnd);
        }

        [Fact]
        public void GetCdnaPosition_Snv_AfterOutFrameRnaEditDeletion()
        {
            // NM_001317107.1
            var observed = MappedPositionUtilities.GetCdnaPositions(_regionsNm1317107[0], _regionsNm1317107[0],
                22138550, 22138550, true);

            Assert.Equal(681, observed.CdnaStart);
        }

        [Fact]
        public void GetCdnaPosition_Snv_AfterInframeRnaEditInsertion()
        {
            // NM_000682.6
            var observed =
                MappedPositionUtilities.GetCdnaPositions(_regionsNm682[0], _regionsNm682[0], 96780984, 96780984, true);

            Assert.Equal(1010, observed.CdnaStart);
        }

        [Fact]
        public void GetCdnaPosition_Snv_AfterOutframeRnaEditInsertion()
        {
            // NM_033517.1
            var observed = MappedPositionUtilities.GetCdnaPositions(_regionsNm33517[20], _regionsNm33517[20], 51135986,
                51135986, false);

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
            (int start, int end) =
                _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, ~_forwardTranscriptRegions.Length, 300, 2, false);
            Assert.Equal(300, start);
            Assert.Equal(556, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_StartBefore_EndAfter()
        {
            (int start, int end) =
                _forwardTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_forwardTranscriptRegions.Length, false);
            Assert.Equal(1, start);
            Assert.Equal(556, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Forward_Insertion_StartAfter_EndExon()
        {
            var regions = new TranscriptRegion[]
            {
                new(10, 19, 10, 19, TranscriptRegionType.Exon, 1, null),
                new(20, 29, 19, 20, TranscriptRegionType.Intron, 1, null),
                new(30, 39, 20, 29, TranscriptRegionType.Exon, 2, null),
                new(40, 49, 29, 30, TranscriptRegionType.Intron, 2, null),
                new(50, 59, 30, 39, TranscriptRegionType.Exon, 3, null),
                new(60, 69, 39, 40, TranscriptRegionType.Intron, 3, null),
                new(70, 79, 40, 49, TranscriptRegionType.Exon, 4, null),
                new(80, 89, 49, 50, TranscriptRegionType.Intron, 4, null),
                new(90, 4834618, 50, 1676, TranscriptRegionType.Exon, 5, null),
                new(4834619, 4842604, 1676, 1677, TranscriptRegionType.Intron, 5, null),
                new(4842605, 4852594, 1677, 11666, TranscriptRegionType.Exon, 6, null)
            };

            (int start, int end) = regions.GetCoveredCdnaPositions(-1, -12, 11666, 1, false);
            Assert.Equal(11666, start);
            Assert.Equal(11666, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndExon()
        {
            var regions = new TranscriptRegion[]
            {
                new(103288513, 103288696, 522, 705, TranscriptRegionType.Exon, 4, null)
            };

            // ClinVar ENST00000546844 103288512
            (int start, int end) = regions.GetCoveredCdnaPositions(523, -1, -1, 0, true);
            Assert.Equal(523, start);
            Assert.Equal(705, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartIntron_EndExon()
        {
            var regions = new TranscriptRegion[]
            {
                new(103271329, 103288512, 825, 826, TranscriptRegionType.Intron, 3, null),
                new(103288513, 103288696, 642, 825, TranscriptRegionType.Exon, 3, null)
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
            var regions = new TranscriptRegion[]
            {
                new(25398208, 25398329, 167, 288, TranscriptRegionType.Exon, 2, null),
                new(25398330, 25403697, 166, 167, TranscriptRegionType.Intron, 1, null)
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
            var regions = new TranscriptRegion[]
            {
                new(103288513, 103288696, 1, 825, TranscriptRegionType.Exon, 1, null)
            };

            // synthetic
            (int start, int end) = regions.GetCoveredCdnaPositions(-1, ~1, -1, -1, true);
            Assert.Equal(1, start);
            Assert.Equal(825, end);
        }

        [Fact]
        public void GetCoveredCdnaPositions_Reverse_StartBefore_EndAfter()
        {
            (int start, int end) =
                _reverseTranscriptRegions.GetCoveredCdnaPositions(-1, -1, -1, ~_reverseTranscriptRegions.Length, true);
            Assert.Equal(1, start);
            Assert.Equal(811, end);
        }

        [Fact]
        public void GetCdsPosition_Forward_Insertion()
        {
            var codingRegion =
                new CodingRegion(78001559, 78024355, 262, 495, string.Empty, string.Empty, 0, 0, 0, null, null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 486, 485, true);

            Assert.Equal(225, cdsStart);
            Assert.Equal(224, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_Snv_AfterOutFrameRnaEditDeletion()
        {
            // NM_001317107.1
            var codingRegion =
                new CodingRegion(22138201, 22139150, 83, 1030, string.Empty, string.Empty, 0, 0, 0, null, null);
            (int cdsStart, _) = MappedPositionUtilities.GetCdsPositions(codingRegion, 681, 681, false);

            Assert.Equal(599, cdsStart);
        }

        [Fact]
        public void GetCdsPosition_Snv_AfterInframeRnaEditInsertion()
        {
            // NM_000682.6
            var codingRegion =
                new CodingRegion(96780545, 96781888, 97, 1449, string.Empty, string.Empty, 0, 0, 0, null, null);
            (int cdsStart, _) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 1010, 1010, false);

            Assert.Equal(914, cdsStart);
        }

        [Fact]
        public void GetCdsPosition_Snv_AfterOutframeRnaEditInsertion()
        {
            // NM_033517.1
            var codingRegion =
                new CodingRegion(51113070, 51169740, 1, 5196, string.Empty, string.Empty, 0, 0, 0, null, null);
            (int cdsStart, _) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 1343, 1343, false);

            Assert.Equal(1343, cdsStart);
        }

        [Fact]
        public void GetCdsPosition_Forward_Insertion_WithCdsOffset()
        {
            var codingRegion = new CodingRegion(6413107, 6415837, 1, 953, string.Empty, string.Empty, 0, 1, 0, null, null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 29, 28, true);

            Assert.Equal(30, cdsStart);
            Assert.Equal(29, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_Reverse_NoCodingRegion_Deletion()
        {
            (int cdsStart, int cdsEnd) = MappedPositionUtilities.GetCdsPositions(null, -1, 123, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Forward()
        {
            // variant: [6647337, 6647336] insertion after coding region
            var codingRegion =
                new CodingRegion(6643999, 6647336, 667, 1674, string.Empty, string.Empty, 0, 0, 0, null, null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 1675, 1674, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionAfterCodingRegion_Reverse()
        {
            // variant: [103629803, 103629804] insertion after coding region
            var codingRegion = new CodingRegion(103113259, 103629803, 161, 10543, string.Empty, string.Empty, 0, 0, 0,
                null, null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 161, 160, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_SilenceOutput_InsertionBeforeCodingRegion_Reverse()
        {
            // variant: [37480320, 37480319] insertion after coding region
            var codingRegion = new CodingRegion(37480320, 37543667, 556, 3228, string.Empty, string.Empty, 0, 0, 0, null,
                null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 3229, 3228, true);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(-1, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_DoNotSilenceOutput_Reverse()
        {
            // variant: [179315139, 179315692]
            var codingRegion = new CodingRegion(179308070, 179315170, 617, 942, string.Empty, string.Empty, 0, 0, 0, null,
                null);

            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(codingRegion, 95, 648, false);

            Assert.Equal(-1, cdsStart);
            Assert.Equal(32, cdsEnd);
        }

        [Fact]
        public void GetCdsPosition_PartialCdsBug_NeedCdsOffset()
        {
            (int cdsStart, int cdsEnd) =
                MappedPositionUtilities.GetCdsPositions(MockedData.CodingRegions.NM_012234_6, 868, 869, false);

            Assert.Equal(685, cdsStart);
            Assert.Equal(686, cdsEnd);
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
        public void GetInsertionCdnaPositions_ExonEndpointInsertion_UndefinedCdnaStart_Reverse()
        {
            var startRegion =
                new TranscriptRegion(243736351, 243776972, 762, 763, TranscriptRegionType.Intron, 7, null);
            var endRegion = new TranscriptRegion(243736228, 243736350, 763, 885, TranscriptRegionType.Exon, 8, null);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion,
                243736351, 243736350, true);

            Assert.Equal(762, cdnaStart);
            Assert.Equal(763, cdnaEnd);
        }

        [Fact]
        public void GetInsertionCdnaPositions_ExonEndpointInsertion_NotObserved_UndefinedCdnaEnd_Reverse()
        {
            var startRegion = new TranscriptRegion(2000, 2199, 1, 200, TranscriptRegionType.Exon, 2, null);
            var endRegion   = new TranscriptRegion(1999, 1000, 200, 201, TranscriptRegionType.Intron, 2, null);

            (int cdnaStart, int cdnaEnd) =
                MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion, 2000, 1999, true);

            Assert.Equal(200, cdnaStart);
            Assert.Equal(201, cdnaEnd);
        }

        [Fact]
        public void GetInsertionCdnaPositions_ExonEndpointInsertion_UndefinedCdnaEnd_Reverse()
        {
            var startRegion = new TranscriptRegion(1333613, 1333722, 396, 505, TranscriptRegionType.Exon, 3, null);
            var endRegion   = new TranscriptRegion(1330895, 1333612, 505, 506, TranscriptRegionType.Intron, 3, null);

            (int cdnaStart, int cdnaEnd) =
                MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion, 1333613, 1333612, true);

            Assert.Equal(505, cdnaStart);
            Assert.Equal(506, cdnaEnd);
        }

        [Fact]
        public void GetInsertionCdnaPositions_ExonEndpointInsertion_UndefinedCdnaStart_Forward()
        {
            var startRegion =
                new TranscriptRegion(89521770, 89528546, 3071, 3072, TranscriptRegionType.Intron, 16, null);
            var endRegion = new TranscriptRegion(89521614, 89521769, 2916, 3071, TranscriptRegionType.Exon, 16, null);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion,
                89521770, 89521769, false);

            Assert.Equal(3072, cdnaStart);
            Assert.Equal(3071, cdnaEnd);
        }

        [Fact]
        public void GetInsertionCdnaPositions_ExonEndpointInsertion_UndefinedCdnaEnd_Forward()
        {
            var startRegion = new TranscriptRegion(99459243, 99459360, 108, 225, TranscriptRegionType.Exon, 2, null);
            var endRegion   = new TranscriptRegion(99456512, 99459242, 107, 108, TranscriptRegionType.Intron, 1, null);

            (int cdnaStart, int cdnaEnd) = MappedPositionUtilities.GetInsertionCdnaPositions(startRegion, endRegion,
                99459243, 99459242, false);

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
        public void GetCoveredCdsAndProteinPositions_AsExpected(int coveredCdnaStart, int coveredCdnaEnd,
            byte cdsOffset, int codingCdnaStart, int codingCdnaEnd, int expectedCdsStart, int expectedCdsEnd,
            int expectedProteinStart, int expectedProteinEnd)
        {
            var codingRegion = new CodingRegion(-1, -1, codingCdnaStart, codingCdnaEnd, string.Empty, string.Empty, 0,
                cdsOffset, 0, null, null);

            var coveredPositions =
                MappedPositionUtilities.GetCoveredCdsAndProteinPositions(coveredCdnaStart, coveredCdnaEnd,
                    codingRegion);
            
            Assert.Equal(expectedCdsStart, coveredPositions.CdsStart);
            Assert.Equal(expectedCdsEnd, coveredPositions.CdsEnd);
            Assert.Equal(expectedProteinStart, coveredPositions.ProteinStart);
            Assert.Equal(expectedProteinEnd, coveredPositions.ProteinEnd);
        }

        [Theory]
        [InlineData(43_364_292, 739)]
        [InlineData(43_364_293, 738)]
        [InlineData(43_364_294, 736)]
        [InlineData(43_364_295, 735)]
        public void GetCigarCdnaPosition_MAP3K14(int position, int expected)
        {
            var cigarOps = new[]
            {
                new CigarOp(CigarType.Match,     118),
                new CigarOp(CigarType.Insertion, 1),
                new CigarOp(CigarType.Match,     496)
            };

            var transcriptRegion =
                new TranscriptRegion(43363798, 43364411, 619, 1233, TranscriptRegionType.Exon, 5, cigarOps);
            int actual = MappedPositionUtilities.GetCigarCdnaPosition(transcriptRegion, position, true);

            Assert.Equal(expected, actual);
        }
    }
}