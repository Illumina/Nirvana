using Moq;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsCodingNomenclatureTests
    {
        private readonly ITranscript _forwardTranscript;
        private readonly ITranscript _reverseTranscript;
        private static readonly IChromosome Chromosome = new Chromosome("chr1", "1", 0);

        public HgvsCodingNomenclatureTests()
        {
            _forwardTranscript = GetForwardTranscript();
            _reverseTranscript = GetReverseTranscript();
        }

        internal static ITranscript GetForwardTranscript()
        {
            // get info from ENST00000343938.4 
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 1260147, 1260482, 1, 336),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 1260483, 1262215, 336, 337),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 1262216, 1262412, 337, 533),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 1262413, 1262620, 533, 534),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 1262621, 1264277, 534, 2190)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(1262291, 1263143, 412, 1056, 645));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000343938", 4));
            transcript.SetupGet(x => x.Chromosome).Returns(Chromosome);
            transcript.SetupGet(x => x.Start).Returns(1260147);
            transcript.SetupGet(x => x.End).Returns(1264277);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);
            transcript.SetupGet(x => x.TotalExonLength).Returns(2190);
            return transcript.Object;
        }

        internal static ITranscript GetReverseTranscript()
        {
            // get info from "ENST00000423372.3
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 134901, 135802, 1760, 2661),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 135803, 137620, 1759, 1760),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 137621, 139379, 1, 1759)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(138530, 139309, 71, 850, 780));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000423372", 3));
            transcript.SetupGet(x => x.Chromosome).Returns(Chromosome);
            transcript.SetupGet(x => x.Start).Returns(134901);
            transcript.SetupGet(x => x.End).Returns(139379);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);
            transcript.SetupGet(x => x.TotalExonLength).Returns(2661);
            return transcript.Object;
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_3UTR()
        {
            var variant       = new SimpleVariant(Chromosome, 1260247, 1260247, "A", "G", VariantType.SNV);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 0, 0);

            Assert.Equal("ENST00000343938.4:c.-311A>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_before_TSS()
        {
            var variant       = new SimpleVariant(Chromosome, 1262210, 1262210, "C", "G", VariantType.SNV);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 1, 1);

            Assert.Equal("ENST00000343938.4:c.-75-6C>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant       = new SimpleVariant(Chromosome, 1262629, 1262628, "", "G", VariantType.insertion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4);

            Assert.Equal("ENST00000343938.4:c.130_131insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_after_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant       = new SimpleVariant(Chromosome, 1263159, 1263158, "", "G", VariantType.insertion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4);

            Assert.Equal("ENST00000343938.4:c.*15_*16insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_duplication_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262626, 2)).Returns("TA");

            var variant       = new SimpleVariant(Chromosome, 1262629, 1262628, "", "TA", VariantType.insertion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4);

            Assert.Equal("ENST00000343938.4:c.129_130dupTA", observedHgvsc);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_NonCoding_Reverse()
        {
            var regions = new ITranscriptRegion[3];
            regions[0] = new TranscriptRegion(TranscriptRegionType.Exon, 2, 20976856, 20977050, 154, 348);
            regions[1] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 20977051, 20977054, 153, 154);
            regions[2] = new TranscriptRegion(TranscriptRegionType.Exon, 1, 20977055, 20977207, 1, 153);

            var observedResults = regions.ShiftDuplication(20977006, "AACT", true);

            Assert.Equal("AACT", observedResults.RefAllele);
            Assert.Equal(20977009, observedResults.Start);
            Assert.Equal(20977006, observedResults.End);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_Coding_Forward()
        {
            var regions = new ITranscriptRegion[41];
            for (int i = 0; i < 22; i++)              regions[i] = new TranscriptRegion(TranscriptRegionType.Exon, 0, 107000000, 107334926, 1, 1564);
            for (int i = 23; i < regions.Length; i++) regions[i] = new TranscriptRegion(TranscriptRegionType.Exon, 0, 107335162, 108000000, 1662, 1700);
            regions[21] = new TranscriptRegion(TranscriptRegionType.Intron, 11, 107334926, 107335065, 1565, 1566);
            regions[22] = new TranscriptRegion(TranscriptRegionType.Exon, 12, 107335066, 107335161, 1566, 1661);

            var observedResults = regions.ShiftDuplication(107335068, "AGTC", false);

            Assert.Equal("AGTC", observedResults.RefAllele);
            Assert.Equal(107335064, observedResults.Start);
            Assert.Equal(107335067, observedResults.End);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_start_before_transcript()
        {
            var variant       = new SimpleVariant(Chromosome, 1260144, 1260148, "ATGTC", "", VariantType.deletion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Delin_start_from_Exon_end_in_intron()
        {
            var variant       = new SimpleVariant(Chromosome, 1262410, 1262414, "ATGTC", "TG", VariantType.indel);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3);

            Assert.Equal("ENST00000343938.4:c.120_122+2delATGTCinsTG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_inversion_start_from_Exon_end_in_intron()
        {
            var variant       = new SimpleVariant(Chromosome, 1262410, 1262414, "ATGTC", "GACAT", VariantType.MNV);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3);

            Assert.Equal("ENST00000343938.4:c.120_122+2invATGTC", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_end_after_transcript()
        {
            var variant       = new SimpleVariant(Chromosome, 1260143, 1260148, "ATGTC", "", VariantType.deletion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Reference_no_hgvsc()
        {
            var variant       = new SimpleVariant(Chromosome, 1260138, 1260138, "A", "A", VariantType.reference);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, -1);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_of_reverse_gene()
        {
            var variant       = new SimpleVariant(Chromosome, 136000, 136000, "A", "G", VariantType.SNV);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 1, 1);

            Assert.Equal("ENST00000423372.3:c.*910-198T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_after_stopCodon_of_reverse_gene()
        {
            var variant       = new SimpleVariant(Chromosome, 138529, 138529, "A", "G", VariantType.SNV);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 2, -1);

            Assert.Equal("ENST00000423372.3:c.*1T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_deletion_of_reverse_gene()
        {
            var variant       = new SimpleVariant(Chromosome, 135802, 137619, "ATCGTGGGTTGT", "", VariantType.deletion);
            var observedHgvsc = HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 0, 1);

            Assert.Equal("ENST00000423372.3:c.*909+2_*910delACAACCCACGAT", observedHgvsc);
        }
    }
}