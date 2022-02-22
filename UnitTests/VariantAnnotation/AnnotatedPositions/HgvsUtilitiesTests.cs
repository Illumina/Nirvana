using Cache.Data;
using Genome;
using UnitTests.TestDataStructures;
using UnitTests.VariantAnnotation.Utilities;
using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsUtilitiesTests
    {
        [Fact]
        public void GetPositionOffset_Intron_RltL_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 137619, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(2, po.Offset);
            Assert.Equal(1759, po.Position);
            Assert.Equal("*909+2", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_ReqL_Reverse()
        {
            var regions = new TranscriptRegion[]
            {
                new(108901173, 108918171, 422, 423, TranscriptRegionType.Intron, 10, null)
            };

            var codingRegion = new CodingRegion(108813927, 108941437, 129, 1613, string.Empty, string.Empty, 0, 0, 0, null,
                null);
            var transcript   = TranscriptMocker.GetTranscript(true, regions, codingRegion, 108810721, 108918171);

            var po = HgvsUtilities.GetPositionOffset(transcript, 108909672, 0);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(8500, po.Offset);
            Assert.Equal(422, po.Position);
            Assert.Equal("294+8500", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_LltR_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 136000, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(-198, po.Offset);
            Assert.Equal(1760, po.Position);
            Assert.Equal("*910-198", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_LeqR_Reverse()
        {
            var regions = new TranscriptRegion[]
            {
                new(134901, 135802, 1760, 2661, TranscriptRegionType.Exon, 2, null),
                new(135803, 137619, 1759, 1760, TranscriptRegionType.Intron, 1, null),
                new(137620, 139379, 1, 1759, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion = new CodingRegion(138530, 139309, 71, 850, string.Empty, string.Empty, 0, 0, 0, null, null);
            var transcript   = TranscriptMocker.GetTranscript(true, regions, codingRegion, 134901, 139379);

            var po = HgvsUtilities.GetPositionOffset(transcript, 136711, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(909, po.Offset);
            Assert.Equal(1759, po.Position);
            Assert.Equal("*909+909", po.Value);
        }

        [Theory]
        [InlineData(1100, 100, "50")] // last genomic position before deletion
        [InlineData(1101, 100, "50")] // start of deletion
        [InlineData(1102, 100, "50")] // middle of deletion
        [InlineData(1103, 100, "50")] // end of deletion
        [InlineData(1104, 101, "51")] // first genomic position after deletion
        public void GetPositionOffset_Deletion_Forward(int genomicPosition, int expectedCdna, string expectedCds)
        {
            var transcript = GetForwardGapTranscript();

            PositionOffset po = HgvsUtilities.GetPositionOffset(transcript, genomicPosition, 0);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(expectedCdna, po.Position);
            Assert.Equal(expectedCds, po.Value);
        }

        [Theory]
        [InlineData(1100, 201, "151")] // first genomic position after deletion
        [InlineData(1101, 200, "150")] // end of deletion
        [InlineData(1102, 200, "150")] // middle of deletion
        [InlineData(1103, 200, "150")] // start of deletion
        [InlineData(1104, 200, "150")] // last genomic position before deletion
        public void GetPositionOffset_Deletion_Reverse(int genomicPosition, int expectedCdna, string expectedCds)
        {
            var transcript = GetReverseGapTranscript();

            PositionOffset po = HgvsUtilities.GetPositionOffset(transcript, genomicPosition, 0);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(expectedCdna, po.Position);
            Assert.Equal(expectedCds, po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_RltL_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 1262210, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(-6, po.Offset);
            Assert.Equal(337, po.Position);
            Assert.Equal("-75-6", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_LltR_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 1260583, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(101, po.Offset);
            Assert.Equal(336, po.Position);
            Assert.Equal("-76+101", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Intron_LeqR_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 1261349, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(867, po.Offset);
            Assert.Equal(336, po.Position);
            Assert.Equal("-76+867", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Exon_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 1262627, 4);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(540, po.Position);
            Assert.Equal("129", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Exon_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po         = HgvsUtilities.GetPositionOffset(transcript, 137721, 2);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(1659, po.Position);
            Assert.Equal("*809", po.Value);
        }

        [Fact]
        public void GetPositionOffset_Gap_Forward_ReturnNull()
        {
            // Not a real example
            // chr1    Ensembl transcript      134901  139379  .       -       .       gene_id "ENSG00000237683"; gene_name "AL627309.1"; transcript_id "ENST00000423372.3"; transcript_type "protein_coding"; tag "canonical"; protein_id "ENSP00000473460.1"; internal_gene_id "10";
            // chr1    Ensembl exon    134901  135802  .       -       .       gene_id "ENSG00000237683"; gene_name "AL627309.1"; transcript_id "ENST00000423372.3"; transcript_type "protein_coding"; tag "canonical"; protein_id "ENSP00000473460.1"; exon_number 2; internal_gene_id "10";
            
            var cigarOps = new CigarOp[]
                {new(CigarType.Match, 6), new(CigarType.Deletion, 902), new(CigarType.Match, 5)};
            
            var regions = new TranscriptRegion[]
            {
                new(134895, 135807, 1755, 1765, TranscriptRegionType.Exon, 1, cigarOps)
            };

            var codingRegion = new CodingRegion(138530, 139309, 71, 850, string.Empty, string.Empty, 0, 0, 0, null, null);
            var transcript   = TranscriptMocker.GetTranscript(false, regions, codingRegion, 134901, 139379);

            var po = HgvsUtilities.GetPositionOffset(transcript, 135001, 0);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(1760, po.Position);
            Assert.Equal("*910", po.Value);
        }

        private static ISequence GetGenomicRefSequence()
        {
            return new SimpleSequence(
                "AGACACAGAAACAGAAACACACAGACAGAAACAAACACAGAGACACAGAGACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACACAGACAGAAACAAACACAGAAACACAGAGACACAGAAATACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAGACACAGAAACACAGCGACGCAGAAACACAGCAACACAAACACAGAAACACAGAAACACACAGAAACAGAAACACAGAAACAAACACAGAGACACAGACACACAGACACAGAGAAACACAGAAACACACAGAAACACTGAAACACAGTGGGCGGTGTCCAGGCTGCAGAGGCTCCATCGCTGT",
                2258580);
        }

        private static Transcript GetGenomicTranscript()
        {
            var regions = new TranscriptRegion[]
            {
                new(2258581, 2259042, 1, 462, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion = new CodingRegion(1000, 1200, 1, 200, string.Empty, string.Empty, 0, 0, 0, null, null);
            return TranscriptMocker.GetTranscript(false, regions, codingRegion, 1001, 1403);
        }

        private static Transcript GetReverseGapTranscript()
        {
            var cigarOps = new CigarOp[]
            {
                new(CigarType.Match, 100),
                new(CigarType.Deletion, 3),
                new(CigarType.Match, 100)
            };

            var regions = new TranscriptRegion[]
            {
                new(1001, 1203, 101, 300, TranscriptRegionType.Exon, 2, cigarOps),
                new(1204, 1303, 100, 101, TranscriptRegionType.Intron, 1, null),
                new(1304, 1403, 1, 100, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion = new CodingRegion(1051, 1353, 51, 250, string.Empty, string.Empty, 0, 0, 0, null, null);
            return TranscriptMocker.GetTranscript(true, regions, codingRegion, 1001, 1403);
        }

        private static Transcript GetForwardGapTranscript()
        {
            var cigarOps = new CigarOp[]
            {
                new(CigarType.Match, 100),
                new(CigarType.Deletion, 3),
                new(CigarType.Match, 100)
            };

            var regions = new TranscriptRegion[]
            {
                new(1001, 1203, 1, 200, TranscriptRegionType.Exon, 1, cigarOps),
                new(1204, 1303, 200, 201, TranscriptRegionType.Intron, 1, null),
                new(1304, 1403, 201, 300, TranscriptRegionType.Exon, 2, null)
            };

            var codingRegion = new CodingRegion(1051, 1353, 51, 250, string.Empty, string.Empty, 0, 0, 0, null, null);
            return TranscriptMocker.GetTranscript(false, regions, codingRegion, 1001, 1403);
        }

        private static Transcript GetRnaEditTranscript()
        {
            // NM_033517.1, SHANK3
            var regions = new TranscriptRegion[]
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

            var codingRegion = new CodingRegion(51113070, 51169740, 1, 5196, string.Empty, string.Empty, 0, 0, 0, null,
                null);
            return TranscriptMocker.GetTranscript(false, regions, codingRegion, 51113070, 51169740);
        }
    }
}