using System;
using Moq;
using Piano;
using UnitTests.Utilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using Vcf;
using Xunit;

namespace UnitTests
{
    //    [Collection("Chromosome 1 collection")]
    //    public sealed class PianoTests
    //    {
    //        private static PianoAnnotationSource GetAnnotationSource(string resourcePath)
    //        {
    //            if (resourcePath == null) return null;

    //            var ndbPath = $"UnitTests.Resources.{resourcePath}.ndb";
    //            var refPath = $"UnitTests.Resources.{resourcePath}.bases";

    //            var ndbStream = ResourceUtilities.GetResourceStream(ndbPath);
    //            var refStream = ResourceUtilities.GetResourceStream(refPath);

    //            var compressedSequence       = new CompressedSequence();
    //            var compressedSequenceReader = new CompressedSequenceReader(refStream, compressedSequence);
    //            return new PianoAnnotationSource(ndbStream, compressedSequenceReader);
    //        }

    //        private static PianoVariant GetVariant(PianoAnnotationSource annotationSource, string vcfLine)
    //        {
    //            //var variant = GetVcfVariant(vcfLine);
    //            return annotationSource?.Annotate(variant);
    //        }

    //        private static VcfVariant GetVcfVariant(string vcfLine, bool isGatkGenomeVcf = false)
    //        {
    //            var fields = vcfLine.Split('\t');
    //            return new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
    //        }

    //        [Fact]
    //        public void MissenseMutationAtTss()
    //        {
    //            Console.WriteLine(Resources.Top);
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1266726	.	A	T	.	.	.");
    //            var expectedOut = "chr1\t1266726\tA\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t1\t.\tM/L\tLGPAVLGLSLWALLH\tstart_lost\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void NonsenseMutation()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267977	.	G	T	.	.	.");
    //            var expectedOut = "chr1\t1267977\tG\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t356\tHLALATDPAFCSALG\tE/*\tREQGLEEDVVGQRCP\tstop_gained\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void SynonymousMutation()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267123	.	C	T	.	.	.");
    //            var expectedOut = "chr1\t1267123\tC\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t99\tNNKSDLLPGLRLGYD\tL\tFDTCSEPVVAMKPSL\tsynonymous_variant\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void FrameshiftInsertion()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267980	.	A	AC	.	.	.");
    //            var expectedOut = "chr1\t1267981\t-\tC\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t357\tLALATDPAFCSALGE\tREQGLEEDVVGQRCPQ/TGAGSGGGRGGPALPAX\t.\tframeshift_variant\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void FrameshiftDeletion()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267980	.	AG	A	.	.	.");
    //            var expectedOut = "chr1\t1267981\tG\t-\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t357\tLALATDPAFCSALGE\tREQGLEEDVVGQRCPQ/RSRVWRRTWWASAARX\t.\tframeshift_variant\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void Frameshift()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267403	.	G	GAC	.	.	.");
    //            var expectedOut = "chr1\t1267404\t-\tAC\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t164-165\tAMVTGKFFSFFLMPQ\tVSYGASMELLSARET/TSATVLAWSC*\t.\tstop_gained,frameshift_variant\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    //        [Fact]
    //        public void DeletionOnExonIntronBoundary()
    //        {
    //            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
    //            var annotatedVariant = GetVariant(annotationSource, "chr1	1267401	.	CAGGTC	C	.	.	.");
    //            var expectedOut = "chr1\t1267402\tAGGTC\t-\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t?-165\tAMVTGKFFSFFLMPQ\t.\tSYGASMELLSARETF\tcoding_sequence_variant\n";
    //            Assert.Equal(expectedOut, annotatedVariant.ToString());
    //        }

    public class PianoTests
    {
        public const string ENST00000343938_genomicSequence = "GAGGGCGGGGCGAGGGCGGGGCGGTGGGCGGGGACGGGGCCCGCACGGCGGCTACGGCCTAGGTGAGCGGCTCGGACTCGGCGGCCGCACCTGCCCAACCCAACCCGCACGGTCCGGAAGTCGCCGAGGGGCCGGGAGCGGGAGGGGACGTCGTCCTAGAGGGCCGGAGCGGGCGGGCGGCCGAGGACCCGGCTCCCGCGCAGGACGGAGCCGTGGCTCAGGTCGGCCCCTCCCCAACACCACCCCGGGCCTCCGCCCCTTCCTGGGCCTCTCGGTGGAGCAGGGACCCGAACCGGTGCCCATCCAGTCCGGTGCCATCTGAAGCCCCCTTCCCAGGTGAGACTCGTAGCGCTCGCTCGACAGGGTCTGGTCCCACCCACAAGGCCTGGGGCGCCGTGGGGCCCCGTCTCCTGCTGGCCCCCCAGCCTGCTGTCAGCCCCCGTGCTCTGTGCTCAGGCCGCCCTCGCGCCCGGCCCTGACCTTGGGCCGTTGGGCTGCCCTGGGAAAGGCCTGGAGGTGTCCTGGGTCACCTTCCTGGGCTGGCAAGCTGCCTGCCTCCTGCACAGCCACTGCCCTTCCTGTTGTTACCGAGCCACCAGCCACAGCTCTGAGAAGCTCCTGGCAGCTTCTGTTTGCCACTGGCTCGAATCTGGGCAGGAAGGCAAGGCCCGCAGAATATCTGGTGACCAAGAAGGAAACCCCAGAGCCTCAGAGACCATCTTCTCAGTGGACAAAATTAAGGCCCGAGGAGGGGAGGGGCGTGCTGGAAGTCTATGGGACTGCATCTTTCTGAGGCCCAGGAGCAGCCATCCCCCACACCTGAAGCCCGGTGAGCTCACATCTGGGGCCTCCGCCTGGTGCCAAGCATGCAACCCAACCTGTGGGGCCTGCAACGCCAGGCTTCAGCACCCTGCAGGCACCAGTGCTCCAGCAGCCTGGGCCACGGGCTGGGCAGGGCTTGCAGCCCATGATCCCTAGTGATGAAGGGCCCAGTCCTAGGGTGCTGAGCAACCTGCCCACCTGCTCCTGGCCAGGAGCTCTCACCACGGCTGGGTGCCCTTCCCCCTCCCCCACCGATGGAGTCCCTGCAGCCAGGGAGGCCAGGACAGGGCTCCCAGCACCAACCGGCCTAGGAACCCCCAGGCCCTCTTCCTGGTCGAGGTGGAATGCAGCTGACTCTCAGGTTCCCCAGAGCAGGTGCGGGCCCGTGGGGCACCCGGGGAGACAGGGCAAGGGTGCTTGGCAACACTCACACAAAGCATGGGTGCCTGGATGTCTGTGGATCTGTGGAGTGACTATGTGAATGCCAGCAGAATCCAAAGCAGGGCCTGGGCCACTCGTGGAAGGCTCCCTAGGGCTAGTACAAGAGCCTCGTGGCAATCTTCTGAGTGGTAAAACCCATCTGTGTGGGACATGGAGTTTCAGCAACAGGAGTGAAAACACGTGTCCATCCATCCAGCAAGTGCCAGCCCTACAGCCTCTTTTCTGCTTTTGGGGATGTAGCAGTGAGGAAGATGGGGCAGCCTGCCCGGCAGCATCCCCCCACCCCCGGCCCCACCTGTCTCTGCTTTCTGCTGTGTCTGTTTTCTTGTCTAGGACTTCAGAACTTCCTGTCTTTGTTGTCATCTGACCCCACCCCAGATGGCTGCTCGCACTCCCCATGCACCCAGATAGATGGCTAGGATGGTGCTTGGCTCTCGGCAGGGGCTTAGTATTTCTCCAGCTGGTAAAAGCAGATACAGCATCTAGAGAGAGAAACAAAAACAAGAAAGCACCAGCAGAGACACCTGCTGCAGACAGCGGGGCCTAGTGGTCTGATAAAGCCAGAGGGGGCCACTCTCGGGGTCAGGGACTGACACGGAGTCAGTGGCCTGATCCACAGGAGGGGCTGTGCCAAGGTCCCTGAATGCGCAATCCTGATGAAGGGTGGGTCAGGGTGGTGTGCCTGAGAGCCTGCGGCTTGGCTGGGAGCAGAGCCAGGCAGCTCCTGGGAGGAAGCTCCATGAGGGGCATGAGTGTTCAGTGAGCGGCAATGGGATCGCAGCTATTTTGTTCCCCTCCACACACAGAAAATGAGCCACAGAGCAAGCTGACCCCAGCGACACAGCCCCCCAGCCCTACTGTATTTCCGTTCCTATCAAAAAATGGATGACTCGGAGACAGGTTTCAATCTGAAAGTCGTCCTGGTCAGTTTCAAGCAGTGTCTCGATGAGAAGGAAGAGGTCTTGCTGGACCCCTACATTGCCAGCTGGAAGGGCCTGGTCAGGTGCGTGTGCCAGGGCTGCCTCCTGAGGTGGGCGCTCCCCTGGCCCGAGTCCCATATGTGGCATCTGCCTCCCGACTGCCTGTCCCCACCAGCTTTGCTGCCCGTTTCCAGATGGGTGTGAGCCCCCGCAGGCTGGGCAGCGTCCCCTGCACCCCAGGCGGGCTGCCCCAGGCCTGGGCGAGGACTCGAGCCCCGCTCCCTTCCACAGGTTTCTGAACAGCCTGGGCACCATCTTCTCATTCATCTCCAAGGACGTGGTCTCCAAGCTGCGGATCATGGAGCGCCTCAGGGGCGGCCCGCAGAGCGAGCACTACCGCAGCCTGCAGGCCATGGTGGCCCACGAGCTGAGCAACCGGCTGGTGGACCTGGAGCGCCGCTCCCACCACCCGGAGTCTGGCTGCCGGACGGTGCTGCGCCTGCACCGCGCCCTGCACTGGCTGCAGCTGTTCCTGGAGGGCCTGCGTACCAGCCCCGAGGACGCACGCACCTCCGCGCTCTGCGCCGACTCCTACAACGCCTCGCTGGCCGCCTACCACCCCTGGGTCGTGCGCCGCGCCGTCACCGTGGCCTTCTGCACGCTGCCCACACGCGAGGTCTTCCTGGAGGCCATGAACGTGGGGCCCCCGGAGCAGGCCGTGCAGATGCTAGGCGAGGCCCTCCCCTTCATCCAGCGTGTCTACAACGTCTCCCAGAAGCTCTACGCCGAGCACTCCCTGCTGGACCTGCCCTAGGGGCGGGAAGCCAGGGCCGCACCGGCTTTCCTGCTGCAGATCTGGGCTGCGGTGGCCAGGGCCGTGAGTCCCGTGGCAGAGCCTTCTGGGCGCTGCGGGAACAGGAGATCCTCTGTCGCCCCTGTGAGCTGAGCTGGTTAGGAACCACAGACTGTGACAGAGAAGGTGGCGACCAGCCCAGAAGAGGCCCACCCTCTCGGTCCGGAACAAGACGCCTCGGCCACGGCTCCCCCTCGGCCTATTACACGCGTGCGCAGCCAGGCCTCGCCAGGGTGCGGTGCAGAGCAGAGCAGGCAGGGGTGGGGGCCGGGCCTGCAAGAGCCCGAAAGGTCGCCACCCCCTAGCCTGTGGGGTGCATCTGCGAACCAGGGTGAAGTCACAGGTCCCGGGGTGTGGAGGCTCCATCCTTTCTCCTTTCTGCCAGCCGATGTGTCCTCATCTCAGGCCCGTGCCTGGGACCCCGTGTCTGCCCAGGTGGGCAGCCTTGAGCCCAGGGGACTCAGTGCCCTCCATGCCCTGGCTGGCAGAAACCCTCAACAGCAGTCTGGGCACTGTGGGGCTCTCCCCGCCTCTCCTGCCTTGTTTGCCCCTCAGCGTGCCAGGCAGACTGGGGGCAGGACAGCCGGAAGCTGAGACCAAGGCTCCTCACAGAAGGGCCCAGGAAGTCCCCGCCCTTGGGACAGCCTCCTCCGTAGCCCCTGCACGGCACCAGTTCCCCGAGGGACGCAGCAGGCCGCCTCCCGCAGCGGCCGTGGGTCTGCACAGCCCAGCCCAGCCCAAGGCCCCCAGGAGCTGGGACTCTGCTACACCCAGTGAAATGCTGTGTCCCTTCTCCCCCGTGCCCCTTGATGCCCCCTCCCCACAGTGCTCAGGAGACCCGTGGGGCACGGAACAGGAGGGTCTGGACCCTGTGGCCCAGCCAAAGGCTACCAGACAGCCACAACCAGCCCAGCCACCATCCAGTGCCTGGGGCCTGGCCACTGGCTCTTCACAGTGGACCCCAGCACCTCGGGGTGGCAGAGGGACGGCCCCCACGGCCCAGCAGACATGCGAGCTTCCAGAGTGCAATCTATGTGATGTCTTCCAACGTTAATAAATCACACAGCCTCCCAGGAGGGAGACGCTGGGGTGCAC";

        public static ITranscript GetMockedTranscriptOnForwardStrand()
        {
            var mockedTranscript = new Mock<ITranscript>(); //get info from ENST00000343938.4
            var chromosome = new Chromosome("chr1", "1", 0);
            var start = 1260147;
            var end = 1264277;

            var introns = new IInterval[]
            {
                new Interval(1260483, 1262215),
                new Interval(1262413, 1262620)
            };

            var cdnaMaps = new ICdnaCoordinateMap[]
            {
                new CdnaCoordinateMap(1260147, 1260482, 1, 336),
                new CdnaCoordinateMap(1262216, 1262412, 337, 533),
                new CdnaCoordinateMap(1262621, 1264277, 534, 2160),
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CdnaCoordinateMap(1262291, 1263143, 412, 1056));
            translation.SetupGet(x => x.ProteinId).Returns(CompactId.Convert("ENST00000343938"));
            translation.SetupGet(x => x.ProteinVersion).Returns(4);
            translation.SetupGet(x => x.PeptideSeq).Returns(
                "MDDSETGFNLKVVLVSFKQCLDEKEEVLLDPYIASWKGLVRFLNSLGTIFSFISKDVVSKLRIMERLRGGPQSEHYRSLQAMVAHELSNRLVDLERRSHHPESGCRTVLRLHRALHWLQLFLEGLRTSPEDARTSALCADSYNASLAAYHPWVVRRAVTVAFCTLPTREVFLEAMNVGPPEQAVQMLGEALPFIQRVYNVSQKLYAEHSLLDLP");

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);
            gene.SetupGet(x => x.EnsemblId).Returns(CompactId.Convert("ENSG00000224051 "));
            gene.SetupGet(x => x.Symbol).Returns("CPTP");
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
        public void MissenseVariant()
        {
            var transcript = GetMockedTranscriptOnForwardStrand();
            var chromosome = new Chromosome("chr1", "1", 0);
            var variant = new Variant(chromosome, 1262295, 1262295, "A", "C", VariantType.SNV, "1:1262295:A>C", false, false, null, null, new AnnotationBehavior(false, false, false, false, false, false));
            var refSequence = new SimpleSequence(ENST00000343938_genomicSequence, 1260147 - 1);
            var result = PianoTranscriptAnnotator.GetAnnotatedTranscript(transcript, variant, refSequence, new AminoAcids(false));
            var expectedResult =
                "CPTP	ENSG000000224051	ENST00000343938.4	ENST00000343938.4	2	M	D/A	DSETGFNLKVVLVSF	missense_variant";
            Assert.Equal(expectedResult, result.ToString());

        }

    }

}