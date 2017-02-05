using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class RefMinorFields
    {
        [Fact]
        public void RefSiteRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr1_789256_789257.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "1	789256	rs3131939	T	.	.	LowGQX	END=789256	GT:GQX:DP:DPF:AD	0:.:0:0:0",
                "END=789256;RefMinor;GMAF=T|0.005192", VcfCommon.InfoIndex);
        }

        [Fact]
        public void VariantSiteRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr1_789256_789257.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "1	789256	rs3131939	T	C	.	LowGQX	.	GT:GQX:DP:DPF:AD	0:.:0:0:0", "GMAF=T|0.005192;AF1000G=0.994808",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void DuplicateEntryRefMinor()
        {
            // the following entry should not get refMinor tag. It has conflicting entries in 1kg and should have no allele frequency related info
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chrX_1389061_1389062.nsa"));
            VcfUtilities.FieldDoesNotContain(saReader,
                "X	1389061	.	A	C	100	PASS	AC=3235", "RefMinor", VcfCommon.InfoIndex);
        }

        [Fact]
        public void MixedAlleleRefMinor()
        {
            var vcfLine = "1	7965489	.	C	.	.	PASS	RefMinor;phyloP=0.178	GT:GQX:DP:DPF	0/0:180:61:2";
            var vcfVariant = VcfUtilities.GetVcfVariant(vcfLine);

            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, Resources.MiniSuppAnnot("chr1_7965489_7965490.nsa"), vcfLine);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            DataUtilities.SetConservationScore(altAllele, "0.178");

            var vcf = new VcfConversion();
            var observedInfoField = vcf.Convert(vcfVariant, annotatedVariant).Split('\t')[VcfCommon.InfoIndex];

            Assert.Equal("phyloP=0.178", observedInfoField);
        }

        [Fact]
        public void MissingRefMinorAnnotation()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr2_193187632_193187633.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "2	193187632	.	G	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	.:.:0:2", "RefMinor;GMAF=G|0.01937",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void SpuriousRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr2_190634103_190634104.nsa"));
            VcfUtilities.FieldEquals(saReader, "2	190634103	.	C	.	.	HighDPFRatio	.", ".",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void SpuriousRefMinor2()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chrX_1619046_1619046.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "X	1619046	.	C	.	.	LowGQX	RefMinor	GT:GQX:DP:DPF	0/0:8:38:12", ".", VcfCommon.InfoIndex);
        }
    }
}
