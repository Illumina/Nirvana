using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.OutputDestinations
{
    [Collection("Chromosome 1 collection")]
    public sealed class LiteVcfWriterTests
    {
        #region members

        private readonly VcfUtilities _vcfUtilities = new VcfUtilities();

        #endregion

        [Fact]
        public void BlankInfoField()
        {
            _vcfUtilities.FieldEquals("chr1	24538137	.	C	.	.	PASS	.	GT:GQX:DP:DPF	0/0:99:34:2", null, ".",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void PotentiatlRefMinor()
        {
            _vcfUtilities.FieldEquals(
                "17	77264	.	G	.	428	PASS	END=77264;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11",
                "chr17_77263_77265.nsa", "END=77264;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3", VcfCommon.InfoIndex);
        }

        [Fact]
        public void NotPotentialRefMinor()
        {
            _vcfUtilities.FieldEquals(
                "17	77264	.	G	.	428	PASS	END=77265;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11",
                "chr17_77263_77265.nsa", "END=77265;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3", VcfCommon.InfoIndex);
        }

        [Fact]
        public void FirstAlleleMissingPhylop()
        {
            const string vcfLine = "1	103188976	rs35710136	CTCTA	ATATA,CTCTC	41	PASS	SNVSB=0.0;SNVHPOL=3;AA=.,a;GMAF=A|0.09465,A|0.4898;AF1000G=.,0.510184;phyloP=-0.094	GT:GQ:GQX:DP:DPF:AD	1/2:63:16:12:1:0,7,5";
            var annotatedVariant = DataUtilities.GetVariant(null as string, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            DataUtilities.SetConservationScore(altAllele, null);

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            DataUtilities.SetConservationScore(altAllele2, "-0.094");

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);

            Assert.Contains("phyloP=.,-0.094", observedVcfLine);
        }

        [Fact]
        public void NoPhylopScores()
        {
            const string vcfLine = "1	103188976	rs35710136	CTCTA	ATATA,CTCTC	41	PASS	SNVSB=0.0;SNVHPOL=3;AA=.,a;GMAF=A|0.09465,A|0.4898;AF1000G=.,0.510184;phyloP=-0.094	GT:GQ:GQX:DP:DPF:AD	1/2:63:16:12:1:0,7,5";
            var annotatedVariant = DataUtilities.GetVariant(null as string, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            DataUtilities.SetConservationScore(altAllele, null);

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            DataUtilities.SetConservationScore(altAllele2, null);

            var observedVcfLine = _vcfUtilities.WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);

            Assert.DoesNotContain("phyloP", observedVcfLine);
        }

        [Fact]
        public void DbSnpOutputTest()
        {
            _vcfUtilities.FieldContains("chr1	115256529	.	T	C	1000	PASS	.	GT	0/1", "chr1_115256529_115256530.nsa",
                "rs11554290", VcfCommon.IdIndex);
        }

        [Fact]
        public void Missing1KgValues()
        {
            _vcfUtilities.FieldEquals(
                "17	505249	.	T	C	35	PASS	SNVSB=0.7;SNVHPOL=5	GT:GQ:GQX:DP:DPF:AD	0/1:34:31:5:1:1,4",
                "chr17_505249_505250.nsa",
                "SNVSB=0.7;SNVHPOL=5;GMAF=C|0.13;AF1000G=0.129992;cosmic=1|COSN16302644,1|COSN6658016",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void DuplicateOneKgFreq()
        {
            _vcfUtilities.FieldEquals(
                "5	29786207	rs150619197	C	.	.	SiteConflict;LowGQX	END=29786207;BLOCKAVG_min30p3a;AF1000G=.,0.994409;GMAF=A|0.9944;RefMinor	GT:GQX:DP:DPF	0:24:9:0",
                "chr5_29786207_29786208.nsa",
                "END=29786207;BLOCKAVG_min30p3a;RefMinor;GMAF=C|0.005591",
                VcfCommon.InfoIndex);
        }
    }
}