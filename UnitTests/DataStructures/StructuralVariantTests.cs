using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class SvVcfParsingTests
    {
        [Fact]
        public void Cnv()
        {
            const string vcfLine = "chr1	713044	DUP_gs_CNV_1_713044_755966	C	<CN0>,<CN2>	100	PASS	AC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045";
            var variant = VcfUtilities.GetVariantFeature(vcfLine).ToString();

            Assert.Contains("reference name: chr1",                    variant);
            Assert.Contains("reference begin: 713044",                 variant);
            Assert.Contains("reference end: 755966",                   variant);
            Assert.Contains("reference allele: C",                     variant);
            Assert.Contains("variant allele:   CN0",                   variant);
            Assert.Contains("reference range:  713045 - 755966",       variant);
            Assert.Contains("variant type:     copy_number_variation", variant);
            Assert.Contains("variant allele:   CN2",                   variant);
        }

        [Fact]
        public void Duplication()
        {
            const string vcfLine = "chr1	115251155	.	G	<DUP>	100	PASS	IMPRECISE;SVTYPE=DUP;END=115258781;SVLEN=7627;CIPOS=-1,1;CIEND=-1,1;DP=2635";
            var variant = VcfUtilities.GetVariantFeature(vcfLine).ToString();
            Assert.Contains("reference allele: G",                     variant);
            Assert.Contains("reference begin: 115251155",              variant);
            Assert.Contains("reference end: 115258781",                variant);
            Assert.Contains("reference name: chr1",                    variant);
            Assert.Contains("reference range:  115251156 - 115258781", variant);
            Assert.Contains("variant allele:   duplication",           variant);
            Assert.Contains("variant type:     duplication",           variant);
        }

        [Fact]
        public void Duplication2()
        {
            const string vcfLine = "chrX	66764988	.	G	<DUP>	100	PASS	IMPRECISE;SVTYPE=DUP;END=66943683;SVLEN=178696;CIPOS=-1,1;CIEND=-1,1;DP=2635";
            var variant = VcfUtilities.GetVariantFeature(vcfLine).ToString();
            Assert.Contains("reference allele: G", variant);
            Assert.Contains("reference begin: 66764988", variant);
            Assert.Contains("reference end: 66943683", variant);
            Assert.Contains("reference name: chrX", variant);
            Assert.Contains("reference range:  66764989 - 66943683", variant);
            Assert.Contains("variant allele:   duplication", variant);
            Assert.Contains("variant type:     duplication", variant);
        }

        [Fact]
        public void Insertion()
        {
            const string vcfLine = "chr22	15883626	P1_MEI_4726	T	<INS>	40	.	SVTYPE=INS;CIPOS=-23,23;IMPRECISE;NOVEL;SVMETHOD=SR;NSF5=1;NSF3=0";
            var variant = VcfUtilities.GetVariantFeature(vcfLine).ToString();
            Assert.Contains("reference allele: T", variant);
            Assert.Contains("reference begin: 15883626", variant);
            Assert.Contains("reference end: 15883626", variant);
            Assert.Contains("reference name: chr22", variant);
            Assert.Contains("reference range:  15883627 - 15883626", variant);
            Assert.Contains("variant allele:   insertion", variant);
            Assert.Contains("variant type:     insertion", variant);
        }
    }
}