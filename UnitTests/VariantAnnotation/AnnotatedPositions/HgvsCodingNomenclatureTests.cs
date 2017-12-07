using Moq;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsCodingNomenclatureTests
    {
        private readonly Mock<ITranscript> _forwardTranscript = new Mock<ITranscript>(); //get info from ENST00000343938.4 
        private readonly Mock<ITranscript> _reverseTranscript = new Mock<ITranscript>(); //get info from "ENST00000423372.3

        public HgvsCodingNomenclatureTests()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            const int start = 1260147;
            const int end = 1264277;

            var introns = new IInterval[]
            {
                new Interval(1260483,1262215),
                new Interval(1262413,1262620)
            };

            var cdnaMaps = new ICdnaCoordinateMap[]
            {
                new CdnaCoordinateMap(1260147,1260482,1,336),
                new CdnaCoordinateMap(1262216,1262412,337,533),
                new CdnaCoordinateMap(1262621,1264277,534,2160)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CdnaCoordinateMap(1262291, 1263143, 412, 1056));

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);

            _forwardTranscript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000343938", 4));
            _forwardTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _forwardTranscript.SetupGet(x => x.Start).Returns(start);
            _forwardTranscript.SetupGet(x => x.End).Returns(end);
            _forwardTranscript.SetupGet(x => x.Gene).Returns(gene.Object);
            _forwardTranscript.SetupGet(x => x.Introns).Returns(introns);
            _forwardTranscript.SetupGet(x => x.CdnaMaps).Returns(cdnaMaps);
            _forwardTranscript.SetupGet(x => x.Translation).Returns(translation.Object);
            _forwardTranscript.SetupGet(x => x.TotalExonLength).Returns(2190);


            //set up reverse transcript
            _reverseTranscript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000423372", 3));
            _reverseTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _reverseTranscript.SetupGet(x => x.Start).Returns(134901);
            _reverseTranscript.SetupGet(x => x.End).Returns(139379);
            _reverseTranscript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            _reverseTranscript.SetupGet(x => x.Introns).Returns(new IInterval[] {new Interval(135803, 137620)});
            _reverseTranscript.SetupGet(x => x.CdnaMaps).Returns(new ICdnaCoordinateMap[]{new CdnaCoordinateMap(134901,135802,1760,2661), new CdnaCoordinateMap(137621,139379,1,1759)});
            _reverseTranscript.SetupGet(x => x.Translation.CodingRegion).Returns(new CdnaCoordinateMap(138530,139309,71,850));
            _reverseTranscript.SetupGet(x => x.TotalExonLength).Returns(2661);
        }
        [Fact]
        public void GetHgvscAnnotation_substitution_in_3UTR()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1260247, 1260247, "A", "G",
                VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000343938.4:c.-311A>G";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intorn_before_TSS()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1262210, 1262210, "C", "G",
                VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000343938.4:c.-75-6C>G";

            Assert.Equal(expHgvs, observedHgvsc);
        }


        [Fact]
        public void GetHgvscAnnotation_insertion_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1262629, 1262628, "", "G",
                VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000343938.4:c.130_131insG";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_after_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1263159, 1263158, "", "G",
                VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000343938.4:c.*15_*16insG";

            Assert.Equal(expHgvs, observedHgvsc);
        }
        [Fact]
        public void GetHgvscAnnotation_duplication_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262626, 2)).Returns("TA");
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1262629, 1262628, "", "TA",
                VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000343938.4:c.129_130dupTA";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_start_before_transcript()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1260144, 1260148, "ATGTC", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);

            Assert.Null(observedHgvsc);
        }


        [Fact]
        public void GetHgvscAnnotation_Delin_start_from_Exon_end_in_intron()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1262410, 1262414, "ATGTC", "TG",
                VariantType.indel);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);

            const string expHgvs = "ENST00000343938.4:c.120_122+2delATGTCinsTG";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_inversion_start_from_Exon_end_in_intron()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1262410, 1262414, "ATGTC", "GACAT",
                VariantType.MNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);

            const string expHgvs = "ENST00000343938.4:c.120_122+2invATGTC";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_end_after_transcript()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1260143, 1260148, "ATGTC", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Reference_no_hgvsc()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 1260138, 1260138, "A", "A",
                VariantType.reference);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript.Object, variant, sequence.Object);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substittion_in_intron_of_reverse_gene()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 136000, 136000, "A", "G",
                VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000423372.3:c.*910-198T>C";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substittion_after_stopCodon_of_reverse_gene()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 138529, 138529, "A", "G",
                VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000423372.3:c.*1T>C";

            Assert.Equal(expHgvs, observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_deeltion_of_reverse_gene()
        {
            var sequence = new Mock<ISequence>();
            var variant = new SimpleVariant(new Chromosome("chr1", "1", 0), 135802, 137619, "ATCGTGGGTTGT", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript.Object, variant, sequence.Object);
            const string expHgvs = "ENST00000423372.3:c.*909+2_*910delACAACCCACGAT";

            Assert.Equal(expHgvs, observedHgvsc);
        }
    }

}