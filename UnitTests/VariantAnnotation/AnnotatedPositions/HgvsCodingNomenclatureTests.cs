using Genome;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsCodingNomenclatureTests
    {
        private readonly ITranscript _forwardTranscript;
        private readonly ITranscript _reverseTranscript;
        private readonly ITranscript _gapTranscript;

        public HgvsCodingNomenclatureTests()
        {
            _forwardTranscript = GetForwardTranscript();
            _reverseTranscript = GetReverseTranscript();
            _gapTranscript     = GetGapTranscript();
        }

        internal static ITranscript GetForwardTranscript()
        {
            // get info from ENST00000343938.4 
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1260147, 1260482, 1,   336),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 1260483, 1262215, 336, 337),
                new TranscriptRegion(TranscriptRegionType.Exon,   2, 1262216, 1262412, 337, 533),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 1262413, 1262620, 533, 534),
                new TranscriptRegion(TranscriptRegionType.Exon,   3, 1262621, 1264277, 534, 2190)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(1262291, 1263143, 412, 1056, 645));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000343938", 4));
            transcript.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            transcript.SetupGet(x => x.Start).Returns(1260147);
            transcript.SetupGet(x => x.End).Returns(1264277);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);
            transcript.SetupGet(x => x.TotalExonLength).Returns(2190);
            return transcript.Object;
        }

        private static ITranscript GetForwardTranscriptWithoutUtr()
        {
            //ENST00000579622.1  chrX:70361035-70361156, non-coding, forward strand, no utr
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 70361035, 70361156, 1, 122)
            };

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000579622", 1));
            transcript.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.ChrX);
            transcript.SetupGet(x => x.Start).Returns(70361035);
            transcript.SetupGet(x => x.End).Returns(70361156);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.TotalExonLength).Returns(122);
            return transcript.Object;
        }

        internal static ITranscript GetReverseTranscript()
        {
            // get info from "ENST00000423372.3
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   2, 134901, 135802, 1760, 2661),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 135803, 137620, 1759, 1760),
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 137621, 139379, 1,    1759)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(138530, 139309, 71, 850, 780));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000423372", 3));
            transcript.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            transcript.SetupGet(x => x.Start).Returns(134901);
            transcript.SetupGet(x => x.End).Returns(139379);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);
            transcript.SetupGet(x => x.TotalExonLength).Returns(2661);
            return transcript.Object;
        }

        private static ITranscript GetGapTranscript()
        {
            //NM_000314.4
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 89623195, 89623860, 1,    666),
                new TranscriptRegion(TranscriptRegionType.Gap,    1, 89623861, 89623861, 666,  667),
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 89623862, 89624305, 667,  1110),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 89624306, 89653781, 1110, 1111),
                new TranscriptRegion(TranscriptRegionType.Exon,   2, 89653782, 89653866, 1111, 1195),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 89653867, 89685269, 1195, 1196),
                new TranscriptRegion(TranscriptRegionType.Exon,   3, 89685270, 89685314, 1196, 1240),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 89685315, 89690802, 1240, 1241),
                new TranscriptRegion(TranscriptRegionType.Exon,   4, 89690803, 89690846, 1241, 1284),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 89690847, 89692769, 1284, 1285),
                new TranscriptRegion(TranscriptRegionType.Exon,   5, 89692770, 89693008, 1285, 1523),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 89693009, 89711874, 1523, 1524),
                new TranscriptRegion(TranscriptRegionType.Exon,   6, 89711875, 89712016, 1524, 1665),
                new TranscriptRegion(TranscriptRegionType.Intron, 6, 89712017, 89717609, 1665, 1666),
                new TranscriptRegion(TranscriptRegionType.Exon,   7, 89717610, 89717776, 1666, 1832),
                new TranscriptRegion(TranscriptRegionType.Intron, 7, 89717777, 89720650, 1832, 1833),
                new TranscriptRegion(TranscriptRegionType.Exon,   8, 89720651, 89720875, 1833, 2057),
                new TranscriptRegion(TranscriptRegionType.Intron, 8, 89720876, 89725043, 2057, 2058),
                new TranscriptRegion(TranscriptRegionType.Exon,   9, 89725044, 89728532, 2058, 5546)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(89624227, 89725229, 1032, 2243, 1212));

            var rnaEdits = new IRnaEdit[3];
            rnaEdits[0] = new RnaEdit(667,  667,  null);
            rnaEdits[1] = new RnaEdit(707,  707,  "C");
            rnaEdits[2] = new RnaEdit(5548, 5547, "AAAAAAAAAAAAAAAAAAAAAAAAAA");

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("NM_000314", 4));
            transcript.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr10);
            transcript.SetupGet(x => x.Start).Returns(89623195);
            transcript.SetupGet(x => x.End).Returns(89728532);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);
            transcript.SetupGet(x => x.TotalExonLength).Returns(5546);
            transcript.SetupGet(x => x.RnaEdits).Returns(rnaEdits);
            return transcript.Object;
        }

        [Theory]
        [InlineData(89623861, 89623861, "T",     "",  "T",   VariantType.deletion, null)]
        [InlineData(89623861, 89623861, "T",     "G", "T",   VariantType.SNV,      null)]
        [InlineData(89623901, 89623901, "G",     "C", "C",   VariantType.SNV,      "NM_000314.4:c.-326=")]
        [InlineData(89623901, 89623901, "G",     "T", "C",   VariantType.SNV,      "NM_000314.4:c.-326C>T")]
        [InlineData(89623861, 89623863, "TGG",   "",  "GG",  VariantType.deletion, "NM_000314.4:c.-365_-364del")]
        [InlineData(89623859, 89623861, "GCT",   "",  "GC",  VariantType.deletion, "NM_000314.4:c.-367_-366del")]
        [InlineData(89623860, 89623862, "CTG",   "",  "CG",  VariantType.deletion, "NM_000314.4:c.-366_-365del")]
        [InlineData(89624304, 89624308, "CTGTA", "",  "CT",  VariantType.deletion, "NM_000314.4:c.78_79+3del")]
        [InlineData(89624308, 89624310, "ATC",   "",  "ATC", VariantType.deletion, "NM_000314.4:c.79+3_79+5del")]
        public void GetHgvscAnnotation_in_intron_gap_substitution(int variantStart, int variantEnd, string reference,
            string alt,
            string transcriptRef, VariantType variantType, string expectedHgvsc)
        {
            var (startIndex, _) =
                MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantStart);
            var (endIndex, _) =
                MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantEnd);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr10, variantStart, variantEnd, reference, alt,
                variantType);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_gapTranscript, variant, null, startIndex, endIndex,
                    transcriptRef, null);

            Assert.Equal(expectedHgvsc, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_3UTR()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260247, 1260247, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 0, 0, null, null);

            Assert.Equal("ENST00000343938.4:c.-311A>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_before_TSS()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262210, 1262210, "C", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 1, 1, null, null);

            Assert.Equal("ENST00000343938.4:c.-75-6C>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262629, 1262628, "", "G", VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.130_131insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_after_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1263159, 1263158, "", "G", VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.*15_*16insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_duplication_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262626, 2)).Returns("TA");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262629, 1262628, "", "TA",
                VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.129_130dup", observedHgvsc);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_NonCoding_Reverse()
        {
            var regions = new ITranscriptRegion[3];
            regions[0] = new TranscriptRegion(TranscriptRegionType.Exon,   2, 20976856, 20977050, 154, 348);
            regions[1] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 20977051, 20977054, 153, 154);
            regions[2] = new TranscriptRegion(TranscriptRegionType.Exon,   1, 20977055, 20977207, 1,   153);

            var observedResults = regions.ShiftDuplication(20977006, "AACT", true);

            Assert.Equal("AACT",   observedResults.RefAllele);
            Assert.Equal(20977009, observedResults.Start);
            Assert.Equal(20977006, observedResults.End);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_Coding_Forward()
        {
            var regions = new ITranscriptRegion[41];
            for (int i = 0; i < 22; i++)
                regions[i] = new TranscriptRegion(TranscriptRegionType.Exon, 0, 107000000, 107334926, 1, 1564);
            for (int i = 23; i < regions.Length; i++)
                regions[i] = new TranscriptRegion(TranscriptRegionType.Exon, 0, 107335162, 108000000, 1662, 1700);
            regions[21] = new TranscriptRegion(TranscriptRegionType.Intron, 11, 107334926, 107335065, 1565, 1566);
            regions[22] = new TranscriptRegion(TranscriptRegionType.Exon,   12, 107335066, 107335161, 1566, 1661);

            var observedResults = regions.ShiftDuplication(107335068, "AGTC", false);

            Assert.Equal("AGTC",    observedResults.RefAllele);
            Assert.Equal(107335064, observedResults.Start);
            Assert.Equal(107335067, observedResults.End);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_start_before_transcript()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260144, 1260148, "ATGTC", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Delins_start_from_Exon_end_in_intron()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262410, 1262414, "ATGTC", "TG",
                VariantType.indel);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3, null, null);

            Assert.Equal("ENST00000343938.4:c.120_122+2delinsTG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_inversion_start_from_Exon_end_in_intron()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262410, 1262414, "ATGTC", "GACAT",
                VariantType.MNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3, null, null);

            Assert.Equal("ENST00000343938.4:c.120_122+2inv", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_end_after_transcript()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260143, 1260148, "ATGTC", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Reference_no_hgvsc()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260138, 1260138, "A", "A",
                VariantType.reference);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, -1, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 136000, 136000, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 1, 1, null, null);

            Assert.Equal("ENST00000423372.3:c.*910-198T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_after_stopCodon_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 138529, 138529, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 2, -1, null, null);

            Assert.Equal("ENST00000423372.3:c.*1T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_deletion_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 135802, 137619, "ATCGTGGGTTGT", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 0, 1, "ACAACCCACGAT",
                    null);

            Assert.Equal("ENST00000423372.3:c.*909+2_*910del", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_at_last_position()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(70361157 - 12, 12)).Returns("TATATATATATA");

            var variant = new SimpleVariant(ChromosomeUtilities.ChrX, 70361157, 70361156, "", "ACACCAGCAGCA",
                VariantType.insertion); //right shifted variant
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(GetForwardTranscriptWithoutUtr(), variant, sequence.Object, 0,
                    0, null, null);

            Assert.Equal("ENST00000579622.1:n.122_123insACACCAGCAGCA", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_duplication_at_last_position()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(70361156 - 4, 4)).Returns("ACAC");

            var variant = new SimpleVariant(ChromosomeUtilities.ChrX, 70361157, 70361156, "", "ACAC",
                VariantType.insertion); //right shifted variant
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(GetForwardTranscriptWithoutUtr(), variant, sequence.Object, 0,
                    0, null, null);

            Assert.Equal("ENST00000579622.1:n.119_122dup", observedHgvsc);
        }
    }
}