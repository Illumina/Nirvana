using System.Collections.Generic;
using Moq;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Vcf;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfReaderUtilsTests
    {
        [Fact]
        public void ParseVcfLine_line_with_only_non_informative_alleles_ignored()
        {
            var vcfLine1 = "chr1	13133	.	T	<*>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var vcfLine2 = "chr1	13133	.	T	*	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var vcfLine3 = "chr1	13133	.	T	<M>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 13133)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            Assert.Null(VcfReaderUtils.ParseVcfLine(vcfLine1, variantFactory, refNameToChromosome));
            Assert.Null(VcfReaderUtils.ParseVcfLine(vcfLine2, variantFactory, refNameToChromosome));
            Assert.Null(VcfReaderUtils.ParseVcfLine(vcfLine3, variantFactory, refNameToChromosome));
        }

        [Fact]
        public void ParseVcfLine_non_informative_alleles_or_NonRef_filtered()
        {
            var vcfLine1 = "chr1	13133	.	T	<*>,G	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var vcfLine2 = "chr1	13133	.	T	*,C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var vcfLine3 = "chr1	13133	.	T	<M>,A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var vcfLine4 = "chr1	13133	.	T	<NON_REF>,A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 13133)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var result1 = VcfReaderUtils.ParseVcfLine(vcfLine1, variantFactory, refNameToChromosome);
            var result2 = VcfReaderUtils.ParseVcfLine(vcfLine2, variantFactory, refNameToChromosome);
            var result3 = VcfReaderUtils.ParseVcfLine(vcfLine3, variantFactory, refNameToChromosome);
            var result4 = VcfReaderUtils.ParseVcfLine(vcfLine4, variantFactory, refNameToChromosome);

            Assert.Equal(new[] { "G" }, result1.AltAlleles);
            Assert.Equal(new[] { "C" }, result2.AltAlleles);
            Assert.Equal(new[] { "A" }, result3.AltAlleles);
            Assert.Equal(new[] { "A" }, result4.AltAlleles);
        }

        [Fact]
        public void ParseVcfLine_line_with_only_NonRef_is_refMinor()
        {
            var vcfLine = "1	10628385	.	C	<NON_REF>	.	LowGQX;HighDPFRatio	END=10628385;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:24:9:18";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 10628385)).Returns(true);
            refMinorProvider.Setup(x => x.GetGlobalMajorAlleleForRefMinor(chromosome, 10628385)).Returns("T");
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var result = VcfReaderUtils.ParseVcfLine(vcfLine, variantFactory, refNameToChromosome);

            Assert.Equal("C", result.RefAllele);
            Assert.Equal(new[] { "<NON_REF>" }, result.AltAlleles);
            Assert.Equal("T", result.Variants[0].RefAllele);
            Assert.Equal("C", result.Variants[0].AltAllele);
        }

        [Fact]
        public void ParseVcfLine_line_with_only_NonRef_is_not_refMinor()
        {
            var vcfLine = "1	10005	.	C	<NON_REF>	.	LowGQX	END=10034;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:3:1:0";
            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 10005)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var result = VcfReaderUtils.ParseVcfLine(vcfLine, variantFactory, refNameToChromosome);

            Assert.Equal("C", result.RefAllele);
            Assert.Equal(new[] { "<NON_REF>" }, result.AltAlleles);
            Assert.Equal("C", result.Variants[0].RefAllele);
            Assert.Equal("<NON_REF>", result.Variants[0].AltAllele);
        }
    }
}