using Piano;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests
{
    [Collection("Chromosome 1 collection")]
    public sealed class PianoTests
    {
        private static PianoAnnotationSource GetAnnotationSource(string resourcePath)
        {
            if (resourcePath == null) return null;

            var ndbPath = $"UnitTests.Resources.{resourcePath}.ndb";
            var refPath = $"UnitTests.Resources.{resourcePath}.bases";

            var ndbStream = ResourceUtilities.GetResourceStream(ndbPath);
            var refStream = ResourceUtilities.GetResourceStream(refPath);

            var compressedSequence       = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(refStream, compressedSequence);
            return new PianoAnnotationSource(ndbStream, compressedSequenceReader);
        }

        private static PianoVariant GetVariant(PianoAnnotationSource annotationSource, string vcfLine)
        {
            var variant = GetVcfVariant(vcfLine);
            return annotationSource?.Annotate(variant);
        }

        private static VcfVariant GetVcfVariant(string vcfLine, bool isGatkGenomeVcf = false)
        {
            var fields = vcfLine.Split('\t');
            return new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
        }

        [Fact]
        public void MissenseMutationAtTss()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1266726	.	A	T	.	.	.");
            var expectedOut = "chr1\t1266726\tA\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t1\t.\tM/L\tLGPAVLGLSLWALLH\tstart_lost\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void NonsenseMutation()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267977	.	G	T	.	.	.");
            var expectedOut = "chr1\t1267977\tG\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t356\tHLALATDPAFCSALG\tE/*\tREQGLEEDVVGQRCP\tstop_gained\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void SynonymousMutation()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267123	.	C	T	.	.	.");
            var expectedOut = "chr1\t1267123\tC\tT\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t99\tNNKSDLLPGLRLGYD\tL\tFDTCSEPVVAMKPSL\tsynonymous_variant\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void FrameshiftInsertion()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267980	.	A	AC	.	.	.");
            var expectedOut = "chr1\t1267981\t-\tC\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t357\tLALATDPAFCSALGE\tREQGLEEDVVGQRCPQ/TGAGSGGGRGGPALPAX\t.\tframeshift_variant\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void FrameshiftDeletion()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267980	.	AG	A	.	.	.");
            var expectedOut = "chr1\t1267981\tG\t-\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t357\tLALATDPAFCSALGE\tREQGLEEDVVGQRCPQ/RSRVWRRTWWASAARX\t.\tframeshift_variant\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void Frameshift()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267403	.	G	GAC	.	.	.");
            var expectedOut = "chr1\t1267404\t-\tAC\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t164-165\tAMVTGKFFSFFLMPQ\tVSYGASMELLSARET/TSATVLAWSC*\t.\tstop_gained,frameshift_variant\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }

        [Fact]
        public void DeletionOnExonIntronBoundary()
        {
            var annotationSource = GetAnnotationSource("ENST00000339381_chr1_Ensembl84.ndb");
            var annotatedVariant = GetVariant(annotationSource, "chr1	1267401	.	CAGGTC	C	.	.	.");
            var expectedOut = "chr1\t1267402\tAGGTC\t-\tTAS1R3\tENSG00000169962\tENST00000339381\tENSP00000344411\t?-165\tAMVTGKFFSFFLMPQ\t.\tSYGASMELLSARETF\tcoding_sequence_variant\n";
            Assert.Equal(expectedOut, annotatedVariant.ToString());
        }
    }
}