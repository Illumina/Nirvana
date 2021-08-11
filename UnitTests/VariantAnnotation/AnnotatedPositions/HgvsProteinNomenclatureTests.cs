using Moq;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Pools;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsProteinNomenclatureTests
    {
        public const string Enst00000343938GenomicSequence = "GAGGGCGGGGCGAGGGCGGGGCGGTGGGCGGGGACGGGGCCCGCACGGCGGCTACGGCCTAGGTGAGCGGCTCGGACTCGGCGGCCGCACCTGCCCAACCCAACCCGCACGGTCCGGAAGTCGCCGAGGGGCCGGGAGCGGGAGGGGACGTCGTCCTAGAGGGCCGGAGCGGGCGGGCGGCCGAGGACCCGGCTCCCGCGCAGGACGGAGCCGTGGCTCAGGTCGGCCCCTCCCCAACACCACCCCGGGCCTCCGCCCCTTCCTGGGCCTCTCGGTGGAGCAGGGACCCGAACCGGTGCCCATCCAGTCCGGTGCCATCTGAAGCCCCCTTCCCAGGTGAGACTCGTAGCGCTCGCTCGACAGGGTCTGGTCCCACCCACAAGGCCTGGGGCGCCGTGGGGCCCCGTCTCCTGCTGGCCCCCCAGCCTGCTGTCAGCCCCCGTGCTCTGTGCTCAGGCCGCCCTCGCGCCCGGCCCTGACCTTGGGCCGTTGGGCTGCCCTGGGAAAGGCCTGGAGGTGTCCTGGGTCACCTTCCTGGGCTGGCAAGCTGCCTGCCTCCTGCACAGCCACTGCCCTTCCTGTTGTTACCGAGCCACCAGCCACAGCTCTGAGAAGCTCCTGGCAGCTTCTGTTTGCCACTGGCTCGAATCTGGGCAGGAAGGCAAGGCCCGCAGAATATCTGGTGACCAAGAAGGAAACCCCAGAGCCTCAGAGACCATCTTCTCAGTGGACAAAATTAAGGCCCGAGGAGGGGAGGGGCGTGCTGGAAGTCTATGGGACTGCATCTTTCTGAGGCCCAGGAGCAGCCATCCCCCACACCTGAAGCCCGGTGAGCTCACATCTGGGGCCTCCGCCTGGTGCCAAGCATGCAACCCAACCTGTGGGGCCTGCAACGCCAGGCTTCAGCACCCTGCAGGCACCAGTGCTCCAGCAGCCTGGGCCACGGGCTGGGCAGGGCTTGCAGCCCATGATCCCTAGTGATGAAGGGCCCAGTCCTAGGGTGCTGAGCAACCTGCCCACCTGCTCCTGGCCAGGAGCTCTCACCACGGCTGGGTGCCCTTCCCCCTCCCCCACCGATGGAGTCCCTGCAGCCAGGGAGGCCAGGACAGGGCTCCCAGCACCAACCGGCCTAGGAACCCCCAGGCCCTCTTCCTGGTCGAGGTGGAATGCAGCTGACTCTCAGGTTCCCCAGAGCAGGTGCGGGCCCGTGGGGCACCCGGGGAGACAGGGCAAGGGTGCTTGGCAACACTCACACAAAGCATGGGTGCCTGGATGTCTGTGGATCTGTGGAGTGACTATGTGAATGCCAGCAGAATCCAAAGCAGGGCCTGGGCCACTCGTGGAAGGCTCCCTAGGGCTAGTACAAGAGCCTCGTGGCAATCTTCTGAGTGGTAAAACCCATCTGTGTGGGACATGGAGTTTCAGCAACAGGAGTGAAAACACGTGTCCATCCATCCAGCAAGTGCCAGCCCTACAGCCTCTTTTCTGCTTTTGGGGATGTAGCAGTGAGGAAGATGGGGCAGCCTGCCCGGCAGCATCCCCCCACCCCCGGCCCCACCTGTCTCTGCTTTCTGCTGTGTCTGTTTTCTTGTCTAGGACTTCAGAACTTCCTGTCTTTGTTGTCATCTGACCCCACCCCAGATGGCTGCTCGCACTCCCCATGCACCCAGATAGATGGCTAGGATGGTGCTTGGCTCTCGGCAGGGGCTTAGTATTTCTCCAGCTGGTAAAAGCAGATACAGCATCTAGAGAGAGAAACAAAAACAAGAAAGCACCAGCAGAGACACCTGCTGCAGACAGCGGGGCCTAGTGGTCTGATAAAGCCAGAGGGGGCCACTCTCGGGGTCAGGGACTGACACGGAGTCAGTGGCCTGATCCACAGGAGGGGCTGTGCCAAGGTCCCTGAATGCGCAATCCTGATGAAGGGTGGGTCAGGGTGGTGTGCCTGAGAGCCTGCGGCTTGGCTGGGAGCAGAGCCAGGCAGCTCCTGGGAGGAAGCTCCATGAGGGGCATGAGTGTTCAGTGAGCGGCAATGGGATCGCAGCTATTTTGTTCCCCTCCACACACAGAAAATGAGCCACAGAGCAAGCTGACCCCAGCGACACAGCCCCCCAGCCCTACTGTATTTCCGTTCCTATCAAAAAATGGATGACTCGGAGACAGGTTTCAATCTGAAAGTCGTCCTGGTCAGTTTCAAGCAGTGTCTCGATGAGAAGGAAGAGGTCTTGCTGGACCCCTACATTGCCAGCTGGAAGGGCCTGGTCAGGTGCGTGTGCCAGGGCTGCCTCCTGAGGTGGGCGCTCCCCTGGCCCGAGTCCCATATGTGGCATCTGCCTCCCGACTGCCTGTCCCCACCAGCTTTGCTGCCCGTTTCCAGATGGGTGTGAGCCCCCGCAGGCTGGGCAGCGTCCCCTGCACCCCAGGCGGGCTGCCCCAGGCCTGGGCGAGGACTCGAGCCCCGCTCCCTTCCACAGGTTTCTGAACAGCCTGGGCACCATCTTCTCATTCATCTCCAAGGACGTGGTCTCCAAGCTGCGGATCATGGAGCGCCTCAGGGGCGGCCCGCAGAGCGAGCACTACCGCAGCCTGCAGGCCATGGTGGCCCACGAGCTGAGCAACCGGCTGGTGGACCTGGAGCGCCGCTCCCACCACCCGGAGTCTGGCTGCCGGACGGTGCTGCGCCTGCACCGCGCCCTGCACTGGCTGCAGCTGTTCCTGGAGGGCCTGCGTACCAGCCCCGAGGACGCACGCACCTCCGCGCTCTGCGCCGACTCCTACAACGCCTCGCTGGCCGCCTACCACCCCTGGGTCGTGCGCCGCGCCGTCACCGTGGCCTTCTGCACGCTGCCCACACGCGAGGTCTTCCTGGAGGCCATGAACGTGGGGCCCCCGGAGCAGGCCGTGCAGATGCTAGGCGAGGCCCTCCCCTTCATCCAGCGTGTCTACAACGTCTCCCAGAAGCTCTACGCCGAGCACTCCCTGCTGGACCTGCCCTAGGGGCGGGAAGCCAGGGCCGCACCGGCTTTCCTGCTGCAGATCTGGGCTGCGGTGGCCAGGGCCGTGAGTCCCGTGGCAGAGCCTTCTGGGCGCTGCGGGAACAGGAGATCCTCTGTCGCCCCTGTGAGCTGAGCTGGTTAGGAACCACAGACTGTGACAGAGAAGGTGGCGACCAGCCCAGAAGAGGCCCACCCTCTCGGTCCGGAACAAGACGCCTCGGCCACGGCTCCCCCTCGGCCTATTACACGCGTGCGCAGCCAGGCCTCGCCAGGGTGCGGTGCAGAGCAGAGCAGGCAGGGGTGGGGGCCGGGCCTGCAAGAGCCCGAAAGGTCGCCACCCCCTAGCCTGTGGGGTGCATCTGCGAACCAGGGTGAAGTCACAGGTCCCGGGGTGTGGAGGCTCCATCCTTTCTCCTTTCTGCCAGCCGATGTGTCCTCATCTCAGGCCCGTGCCTGGGACCCCGTGTCTGCCCAGGTGGGCAGCCTTGAGCCCAGGGGACTCAGTGCCCTCCATGCCCTGGCTGGCAGAAACCCTCAACAGCAGTCTGGGCACTGTGGGGCTCTCCCCGCCTCTCCTGCCTTGTTTGCCCCTCAGCGTGCCAGGCAGACTGGGGGCAGGACAGCCGGAAGCTGAGACCAAGGCTCCTCACAGAAGGGCCCAGGAAGTCCCCGCCCTTGGGACAGCCTCCTCCGTAGCCCCTGCACGGCACCAGTTCCCCGAGGGACGCAGCAGGCCGCCTCCCGCAGCGGCCGTGGGTCTGCACAGCCCAGCCCAGCCCAAGGCCCCCAGGAGCTGGGACTCTGCTACACCCAGTGAAATGCTGTGTCCCTTCTCCCCCGTGCCCCTTGATGCCCCCTCCCCACAGTGCTCAGGAGACCCGTGGGGCACGGAACAGGAGGGTCTGGACCCTGTGGCCCAGCCAAAGGCTACCAGACAGCCACAACCAGCCCAGCCACCATCCAGTGCCTGGGGCCTGGCCACTGGCTCTTCACAGTGGACCCCAGCACCTCGGGGTGGCAGAGGGACGGCCCCCACGGCCCAGCAGACATGCGAGCTTCCAGAGTGCAATCTATGTGATGTCTTCCAACGTTAATAAATCACACAGCCTCCCAGGAGGGAGACGCTGGGGTGCAC";

        public static ITranscript GetMockedTranscriptOnForwardStrand()
        {
            var mockedTranscript = new Mock<ITranscript>(); //get info from ENST00000343938.4
            const int start      = 1260147;
            const int end        = 1264277;

            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 1260147, 1260482, 1, 336),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 1260483, 1262215, 336, 337),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 1262216, 1262412, 337, 533),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 1262413, 1262620, 533,534),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 1262621, 1264277, 534, 2190)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(1262291, 1263143, 412, 1056, 645));
            translation.SetupGet(x => x.ProteinId).Returns(CompactId.Convert("ENST00000343938", 4));
            translation.SetupGet(x => x.PeptideSeq).Returns("MDDSETGFNLKVVLVSFKQCLDEKEEVLLDPYIASWKGLVRFLNSLGTIFSFISKDVVSKLRIMERLRGGPQSEHYRSLQAMVAHELSNRLVDLERRSHHPESGCRTVLRLHRALHWLQLFLEGLRTSPEDARTSALCADSYNASLAAYHPWVVRRAVTVAFCTLPTREVFLEAMNVGPPEQAVQMLGEALPFIQRVYNVSQKLYAEHSLLDLP");

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);
            gene.SetupGet(x => x.EnsemblId).Returns(CompactId.Convert("ENSG00000224051 "));

            mockedTranscript.SetupGet(x => x.Id).Returns(CompactId.Convert("ENST00000343938", 4));
            mockedTranscript.SetupGet(x => x.Source).Returns(Source.Ensembl);
            mockedTranscript.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            mockedTranscript.SetupGet(x => x.Start).Returns(start);
            mockedTranscript.SetupGet(x => x.End).Returns(end);
            mockedTranscript.SetupGet(x => x.Gene).Returns(gene.Object);
            mockedTranscript.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions);
            mockedTranscript.SetupGet(x => x.Translation).Returns(translation.Object);
            mockedTranscript.SetupGet(x => x.TotalExonLength).Returns(2190);

            return mockedTranscript.Object;
        }

        [Fact]
        public void GetHgvsProteinAnnotation_substitution()
        {
            var variant     = VariantPool.Get(ChromosomeUtilities.Chr1, 1262295, 1262295, "A", "C", VariantType.SNV, "1:1262295:A>C", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp2Ala)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_insertion()
        {
            var variant     = VariantPool.Get(ChromosomeUtilities.Chr1, 1262297, 1262296, "", "TTC", VariantType.insertion, "1:1262295:T>TTTC", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp2_Asp3insPhe)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_duplication_right_shifted()
        {
            var variant     = VariantPool.Get(ChromosomeUtilities.Chr1, 1262297, 1262296, "", "GAC", VariantType.insertion, "1:1262295:T>GAC", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Asp3dup)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_deletion()
        {
            var variant     = VariantPool.Get(ChromosomeUtilities.Chr1, 1262300, 1262302, "TCG", "", VariantType.deletion, "1:1262300:1262302", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4del)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_delIns()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 1262300, 1262305, "TCGGAG", "GAGACA", VariantType.indel, "1:1262300:1262305", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4_Glu5delinsGluThr)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_no_change()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 1262300, 1262302, "TCG", "AGT", VariantType.indel, "1:1262300:1262302", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:c.10_12delinsAGT(p.(Ser4=))", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_frameshift()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 1262300, 1262301, "TC", "", VariantType.deletion, "1:1262300:1262301", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ser4GlyfsTer19)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_frameshift_stop_gain()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 1262313, 1262312, "", "GA", VariantType.insertion, "1:1262333:1262332", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Phe8Ter)", hgvspNotation);
            VariantPool.Return(variant);
        }

        [Fact]
        public void GetHgvsProteinAnnotation_extension()
        {
            var variant = VariantPool.Get(ChromosomeUtilities.Chr1, 1263141, 1263143, "TAG", "", VariantType.deletion, "1:1263141:1263143", false, false, false, null, AnnotationBehavior.SmallVariants, false);
            var refSequence = new SimpleSequence(Enst00000343938GenomicSequence, 1260147 - 1);
            var transcript  = GetMockedTranscriptOnForwardStrand();

            var annotatedTranscript = FullTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, null, null, new AminoAcids(false));

            var hgvspNotation = annotatedTranscript.HgvsProtein;

            Assert.Equal("ENST00000343938.4:p.(Ter215GlyextTer43)", hgvspNotation);
            VariantPool.Return(variant);
        }
    }
}