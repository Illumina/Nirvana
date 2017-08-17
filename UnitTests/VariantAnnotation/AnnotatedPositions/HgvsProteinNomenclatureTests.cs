using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using VariantAnnotation.TranscriptAnnotation;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsProteinNomenclatureTests
    {
        public const string ENST00000343938_genomicSequence = "GAGGGCGGGGCGAGGGCGGGGCGGTGGGCGGGGACGGGGCCCGCACGGCGGCTACGGCCTAGGTGAGCGGCTCGGACTCGGCGGCCGCACCTGCCCAACCCAACCCGCACGGTCCGGAAGTCGCCGAGGGGCCGGGAGCGGGAGGGGACGTCGTCCTAGAGGGCCGGAGCGGGCGGGCGGCCGAGGACCCGGCTCCCGCGCAGGACGGAGCCGTGGCTCAGGTCGGCCCCTCCCCAACACCACCCCGGGCCTCCGCCCCTTCCTGGGCCTCTCGGTGGAGCAGGGACCCGAACCGGTGCCCATCCAGTCCGGTGCCATCTGAAGCCCCCTTCCCAGGTGAGACTCGTAGCGCTCGCTCGACAGGGTCTGGTCCCACCCACAAGGCCTGGGGCGCCGTGGGGCCCCGTCTCCTGCTGGCCCCCCAGCCTGCTGTCAGCCCCCGTGCTCTGTGCTCAGGCCGCCCTCGCGCCCGGCCCTGACCTTGGGCCGTTGGGCTGCCCTGGGAAAGGCCTGGAGGTGTCCTGGGTCACCTTCCTGGGCTGGCAAGCTGCCTGCCTCCTGCACAGCCACTGCCCTTCCTGTTGTTACCGAGCCACCAGCCACAGCTCTGAGAAGCTCCTGGCAGCTTCTGTTTGCCACTGGCTCGAATCTGGGCAGGAAGGCAAGGCCCGCAGAATATCTGGTGACCAAGAAGGAAACCCCAGAGCCTCAGAGACCATCTTCTCAGTGGACAAAATTAAGGCCCGAGGAGGGGAGGGGCGTGCTGGAAGTCTATGGGACTGCATCTTTCTGAGGCCCAGGAGCAGCCATCCCCCACACCTGAAGCCCGGTGAGCTCACATCTGGGGCCTCCGCCTGGTGCCAAGCATGCAACCCAACCTGTGGGGCCTGCAACGCCAGGCTTCAGCACCCTGCAGGCACCAGTGCTCCAGCAGCCTGGGCCACGGGCTGGGCAGGGCTTGCAGCCCATGATCCCTAGTGATGAAGGGCCCAGTCCTAGGGTGCTGAGCAACCTGCCCACCTGCTCCTGGCCAGGAGCTCTCACCACGGCTGGGTGCCCTTCCCCCTCCCCCACCGATGGAGTCCCTGCAGCCAGGGAGGCCAGGACAGGGCTCCCAGCACCAACCGGCCTAGGAACCCCCAGGCCCTCTTCCTGGTCGAGGTGGAATGCAGCTGACTCTCAGGTTCCCCAGAGCAGGTGCGGGCCCGTGGGGCACCCGGGGAGACAGGGCAAGGGTGCTTGGCAACACTCACACAAAGCATGGGTGCCTGGATGTCTGTGGATCTGTGGAGTGACTATGTGAATGCCAGCAGAATCCAAAGCAGGGCCTGGGCCACTCGTGGAAGGCTCCCTAGGGCTAGTACAAGAGCCTCGTGGCAATCTTCTGAGTGGTAAAACCCATCTGTGTGGGACATGGAGTTTCAGCAACAGGAGTGAAAACACGTGTCCATCCATCCAGCAAGTGCCAGCCCTACAGCCTCTTTTCTGCTTTTGGGGATGTAGCAGTGAGGAAGATGGGGCAGCCTGCCCGGCAGCATCCCCCCACCCCCGGCCCCACCTGTCTCTGCTTTCTGCTGTGTCTGTTTTCTTGTCTAGGACTTCAGAACTTCCTGTCTTTGTTGTCATCTGACCCCACCCCAGATGGCTGCTCGCACTCCCCATGCACCCAGATAGATGGCTAGGATGGTGCTTGGCTCTCGGCAGGGGCTTAGTATTTCTCCAGCTGGTAAAAGCAGATACAGCATCTAGAGAGAGAAACAAAAACAAGAAAGCACCAGCAGAGACACCTGCTGCAGACAGCGGGGCCTAGTGGTCTGATAAAGCCAGAGGGGGCCACTCTCGGGGTCAGGGACTGACACGGAGTCAGTGGCCTGATCCACAGGAGGGGCTGTGCCAAGGTCCCTGAATGCGCAATCCTGATGAAGGGTGGGTCAGGGTGGTGTGCCTGAGAGCCTGCGGCTTGGCTGGGAGCAGAGCCAGGCAGCTCCTGGGAGGAAGCTCCATGAGGGGCATGAGTGTTCAGTGAGCGGCAATGGGATCGCAGCTATTTTGTTCCCCTCCACACACAGAAAATGAGCCACAGAGCAAGCTGACCCCAGCGACACAGCCCCCCAGCCCTACTGTATTTCCGTTCCTATCAAAAAATGGATGACTCGGAGACAGGTTTCAATCTGAAAGTCGTCCTGGTCAGTTTCAAGCAGTGTCTCGATGAGAAGGAAGAGGTCTTGCTGGACCCCTACATTGCCAGCTGGAAGGGCCTGGTCAGGTGCGTGTGCCAGGGCTGCCTCCTGAGGTGGGCGCTCCCCTGGCCCGAGTCCCATATGTGGCATCTGCCTCCCGACTGCCTGTCCCCACCAGCTTTGCTGCCCGTTTCCAGATGGGTGTGAGCCCCCGCAGGCTGGGCAGCGTCCCCTGCACCCCAGGCGGGCTGCCCCAGGCCTGGGCGAGGACTCGAGCCCCGCTCCCTTCCACAGGTTTCTGAACAGCCTGGGCACCATCTTCTCATTCATCTCCAAGGACGTGGTCTCCAAGCTGCGGATCATGGAGCGCCTCAGGGGCGGCCCGCAGAGCGAGCACTACCGCAGCCTGCAGGCCATGGTGGCCCACGAGCTGAGCAACCGGCTGGTGGACCTGGAGCGCCGCTCCCACCACCCGGAGTCTGGCTGCCGGACGGTGCTGCGCCTGCACCGCGCCCTGCACTGGCTGCAGCTGTTCCTGGAGGGCCTGCGTACCAGCCCCGAGGACGCACGCACCTCCGCGCTCTGCGCCGACTCCTACAACGCCTCGCTGGCCGCCTACCACCCCTGGGTCGTGCGCCGCGCCGTCACCGTGGCCTTCTGCACGCTGCCCACACGCGAGGTCTTCCTGGAGGCCATGAACGTGGGGCCCCCGGAGCAGGCCGTGCAGATGCTAGGCGAGGCCCTCCCCTTCATCCAGCGTGTCTACAACGTCTCCCAGAAGCTCTACGCCGAGCACTCCCTGCTGGACCTGCCCTAGGGGCGGGAAGCCAGGGCCGCACCGGCTTTCCTGCTGCAGATCTGGGCTGCGGTGGCCAGGGCCGTGAGTCCCGTGGCAGAGCCTTCTGGGCGCTGCGGGAACAGGAGATCCTCTGTCGCCCCTGTGAGCTGAGCTGGTTAGGAACCACAGACTGTGACAGAGAAGGTGGCGACCAGCCCAGAAGAGGCCCACCCTCTCGGTCCGGAACAAGACGCCTCGGCCACGGCTCCCCCTCGGCCTATTACACGCGTGCGCAGCCAGGCCTCGCCAGGGTGCGGTGCAGAGCAGAGCAGGCAGGGGTGGGGGCCGGGCCTGCAAGAGCCCGAAAGGTCGCCACCCCCTAGCCTGTGGGGTGCATCTGCGAACCAGGGTGAAGTCACAGGTCCCGGGGTGTGGAGGCTCCATCCTTTCTCCTTTCTGCCAGCCGATGTGTCCTCATCTCAGGCCCGTGCCTGGGACCCCGTGTCTGCCCAGGTGGGCAGCCTTGAGCCCAGGGGACTCAGTGCCCTCCATGCCCTGGCTGGCAGAAACCCTCAACAGCAGTCTGGGCACTGTGGGGCTCTCCCCGCCTCTCCTGCCTTGTTTGCCCCTCAGCGTGCCAGGCAGACTGGGGGCAGGACAGCCGGAAGCTGAGACCAAGGCTCCTCACAGAAGGGCCCAGGAAGTCCCCGCCCTTGGGACAGCCTCCTCCGTAGCCCCTGCACGGCACCAGTTCCCCGAGGGACGCAGCAGGCCGCCTCCCGCAGCGGCCGTGGGTCTGCACAGCCCAGCCCAGCCCAAGGCCCCCAGGAGCTGGGACTCTGCTACACCCAGTGAAATGCTGTGTCCCTTCTCCCCCGTGCCCCTTGATGCCCCCTCCCCACAGTGCTCAGGAGACCCGTGGGGCACGGAACAGGAGGGTCTGGACCCTGTGGCCCAGCCAAAGGCTACCAGACAGCCACAACCAGCCCAGCCACCATCCAGTGCCTGGGGCCTGGCCACTGGCTCTTCACAGTGGACCCCAGCACCTCGGGGTGGCAGAGGGACGGCCCCCACGGCCCAGCAGACATGCGAGCTTCCAGAGTGCAATCTATGTGATGTCTTCCAACGTTAATAAATCACACAGCCTCCCAGGAGGGAGACGCTGGGGTGCAC";

        public static ITranscript GetMockedTranscriptOnForwardStrand()
        {
            var mockedTranscript = new Mock<ITranscript>(); //get info from ENST00000343938.4
            var chromosome       = new Chromosome("chr1", "1", 0);
            var start            = 1260147;
            var end              = 1264277;

            var introns = new IInterval[]
            {
                new Interval(1260483,1262215),
                new Interval(1262413,1262620)
            };

            var cdnaMaps = new ICdnaCoordinateMap[]
            {
                new CdnaCoordinateMap(1260147,1260482,1,336),
                new CdnaCoordinateMap(1262216,1262412,337,533),
                new CdnaCoordinateMap(1262621,1264277,534,2160),
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CdnaCoordinateMap(1262291, 1263143, 412, 1056));
            translation.SetupGet(x => x.ProteinId).Returns(CompactId.Convert("ENST00000343938"));
            translation.SetupGet(x => x.ProteinVersion).Returns(4);
            translation.SetupGet(x => x.PeptideSeq).Returns("MDDSETGFNLKVVLVSFKQCLDEKEEVLLDPYIASWKGLVRFLNSLGTIFSFISKDVVSKLRIMERLRGGPQSEHYRSLQAMVAHELSNRLVDLERRSHHPESGCRTVLRLHRALHWLQLFLEGLRTSPEDARTSALCADSYNASLAAYHPWVVRRAVTVAFCTLPTREVFLEAMNVGPPEQAVQMLGEALPFIQRVYNVSQKLYAEHSLLDLP");

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);
            gene.SetupGet(x => x.EnsemblId).Returns(CompactId.Convert("ENSG00000224051 "));

            mockedTranscript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000343938"));
            mockedTranscript.SetupGet(x => x.Source).Returns(Source.Ensembl);
            mockedTranscript.SetupGet(x => x.Version).Returns(4);
            mockedTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            mockedTranscript.SetupGet(x => x.Start).Returns(start);
            mockedTranscript.SetupGet(x => x.End).Returns(end);
            mockedTranscript.SetupGet(x => x.Gene).Returns(gene.Object);
            mockedTranscript.SetupGet(x => x.Introns).Returns(introns);
            mockedTranscript.SetupGet(x => x.CdnaMaps).Returns(cdnaMaps);
            mockedTranscript.SetupGet(x => x.Translation).Returns(translation.Object);
            mockedTranscript.SetupGet(x => x.TotalExonLength).Returns(2190);

            return mockedTranscript.Object;

        }

        [Fact]
        public void GetHgvsProteinAnnotation_substitution()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            var variant     = new Variant(chromosome, 1262295, 1262295, "A", "C", VariantType.SNV, "1:1262295:A>C", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence,null,null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp2Ala)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_insertion()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            var variant     = new Variant(chromosome, 1262297, 1262296, "", "TTC", VariantType.insertion, "1:1262295:T>TTTC", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp2_Asp3insPhe)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_duplication_right_shifted()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            var variant     = new Variant(chromosome, 1262297, 1262296, "", "GAC", VariantType.insertion, "1:1262295:T>GAC", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp3dup)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_deletion()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            var variant     = new Variant(chromosome, 1262300, 1262302, "TCG", "", VariantType.deletion, "1:1262300:1262302", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4del)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_delIns()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1262300, 1262305, "TCGGAG", "GAGACA", VariantType.indel, "1:1262300:1262305", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4_Glu5delinsGluThr)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_no_change()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1262300, 1262302, "TCG", "AGT", VariantType.indel, "1:1262300:1262302", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:c.10_12delTCGinsAGT(p.(Ser4=))", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_frameshift()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1262300, 1262301, "TC", "", VariantType.deletion, "1:1262300:1262301", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4GlyfsTer19)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_frameshift_stop_gain()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1262313, 1262312, "", "GA", VariantType.insertion, "1:1262333:1262332", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Phe8Ter)", hgvspNotation);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_extension()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1263141, 1263143, "TAG", "", VariantType.deletion, "1:1263141:1263143", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null); 

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ter215AlaextTer9)", hgvspNotation);
        }
    }
}