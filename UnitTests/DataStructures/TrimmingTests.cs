using System.Linq;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class TrimmingTests
    {
        [Fact]
        public void Deletion()
        {
            const string vcfLine = "1	100	.	ACT	A	.	.	.";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);
            var altAllele = variant.AlternateAlleles.First();

            Assert.Equal(101, altAllele.ReferenceBegin);
            Assert.Equal(102, altAllele.ReferenceEnd);
            Assert.Equal("CT", altAllele.ReferenceAllele);
            Assert.Equal("", altAllele.AlternateAllele);
        }

        [Fact]
        public void Insertion()
        {
            const string vcfLine = "1	100	.	A	ACT	.	.	.";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);
            var altAllele = variant.AlternateAlleles.First();

            Assert.Equal(101, altAllele.ReferenceBegin);
            Assert.Equal(100, altAllele.ReferenceEnd);
            Assert.Equal("", altAllele.ReferenceAllele);
            Assert.Equal("CT", altAllele.AlternateAllele);
        }

        [Fact]
        public void Mnv()
        {
            const string vcfLine = "1	100	.	ACT	GAC	.	.	.";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);
            var altAllele = variant.AlternateAlleles.First();

            Assert.Equal(100, altAllele.ReferenceBegin);
            Assert.Equal(102, altAllele.ReferenceEnd);
            Assert.Equal("ACT", altAllele.ReferenceAllele);
            Assert.Equal("GAC", altAllele.AlternateAllele);
        }

        [Fact]
        public void MultipleAlleleTrimming()
        {
            const string vcfLine = "17\t2888571\t.\tATGT\tAT,ATG\t24\tLowGQX\tCIGAR=1M2D1M,3M1D;RU=TG,T;REFREP=1,13;IDREP=0,12;CSQT=-|RAP1GAP2|ENST00000254695|intron_variant&feature_truncation,ATG|RAP1GAP2|ENST00000254695|intron_variant\tGT:GQ:GQX:DPI:AD\t1/2:636:596:26:1,14,9";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);

            foreach (var altAllele in variant.AlternateAlleles)
            {
                Assert.Equal("", altAllele.AlternateAllele);
            }
        }

        [Fact]
        public void Snv()
        {
            const string vcfLine = "1	100	.	A	G	.	.	.";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);
            var altAllele = variant.AlternateAlleles.First();

            Assert.Equal(100, altAllele.ReferenceBegin);
            Assert.Equal(100, altAllele.ReferenceEnd);
            Assert.Equal("A", altAllele.ReferenceAllele);
            Assert.Equal("G", altAllele.AlternateAllele);
        }

        [Fact]
        public void TrimBothEnds()
        {
            const string vcfLine = "chr1	100	.	ACTGA	AGTCA	.	.	.";
            var variant = VcfUtilities.GetVariantFeature(vcfLine);
            var altAllele = variant.AlternateAlleles.First();

            Assert.Equal(101, altAllele.ReferenceBegin);
            Assert.Equal(103, altAllele.ReferenceEnd);
            Assert.Equal("CTG", altAllele.ReferenceAllele);
            Assert.Equal("GTC", altAllele.AlternateAllele);
        }
    }
}