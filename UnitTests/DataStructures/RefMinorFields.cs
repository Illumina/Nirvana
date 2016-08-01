using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class RefMinorFields
    {
        #region members

        private readonly VcfUtilities _vcfUtilities = new VcfUtilities();

        #endregion

        [Fact]
        public void RefSiteRefMinor()
        {
            _vcfUtilities.FieldEquals("1	789256	rs3131939	T	.	.	LowGQX	END=789256	GT:GQX:DP:DPF:AD	0:.:0:0:0",
                "chr1_789256_789257.nsa", "END=789256;RefMinor;GMAF=T|0.005192", VcfCommon.InfoIndex);
        }

        [Fact]
        public void VariantSiteRefMinor()
        {
            _vcfUtilities.FieldEquals("1	789256	rs3131939	T	C	.	LowGQX	.	GT:GQX:DP:DPF:AD	0:.:0:0:0",
                "chr1_789256_789257.nsa", "GMAF=T|0.005192;AF1000G=0.994808", VcfCommon.InfoIndex);
        }

        [Fact]
        public void DuplicateEntryRefMinor()
        {
            // the following entry should not get refMinor tag. It has conflicting entries in 1kg and should have no allele frequency related info
            _vcfUtilities.FieldDoesNotContain("X	1389061	.	A	C	100	PASS	AC=3235",
                "chrX_1389061_1389062.nsa", "RefMinor", VcfCommon.InfoIndex);
        }

        [Fact]
        public void MixedAlleleRefMinor()
        {
            var vcfLine = "1	7965489	.	C	.	.	PASS	RefMinor;phyloP=0.178	GT:GQX:DP:DPF	0/0:180:61:2";
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_7965489_7965490.nsa", vcfLine);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            DataUtilities.SetConservationScore(altAllele, "0.178");

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);
            var observedInfoField = observedVcfLine.Split('\t')[VcfCommon.InfoIndex];
            Assert.Equal("phyloP=0.178", observedInfoField);
        }

        [Fact]
        public void MissingRefMinorAnnotation()
        {
            _vcfUtilities.FieldEquals("2	193187632	.	G	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	.:.:0:2",
                "chr2_193187632_193187633.nsa", "RefMinor;GMAF=G|0.01937", VcfCommon.InfoIndex);
        }

        [Fact]
        public void SpuriousRefMinor()
        {
            _vcfUtilities.FieldEquals("2	190634103	.	C	.	.	HighDPFRatio	.",
                "chr2_190634103_190634104.nsa", ".", VcfCommon.InfoIndex);
        }

        [Fact]
        public void SpuriousRefMinor2()
        {
            _vcfUtilities.FieldEquals("X	1619046	.	C	.	.	LowGQX	RefMinor	GT:GQX:DP:DPF	0/0:8:38:12",
                "chrX_1619046_1619046.nsa", ".", VcfCommon.InfoIndex);
        }
    }
}
