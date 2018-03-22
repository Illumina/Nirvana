using System.Collections.Generic;
using System.Linq;
using Moq;
using VariantAnnotation;
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
        public void ParseVcfLine_line_with_only_non_informative_alleles_position_unchanged_but_variants_ignored()
        {
            const string vcfLine1 = "chr1	13133	.	T	<*>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine2 = "chr1	13133	.	T	*	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine3 = "chr1	13133	.	T	<M>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 13133)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var position1 = VcfReaderUtils.ParseVcfLine(vcfLine1, variantFactory, refNameToChromosome);
            var position2 = VcfReaderUtils.ParseVcfLine(vcfLine2, variantFactory, refNameToChromosome);
            var position3 = VcfReaderUtils.ParseVcfLine(vcfLine3, variantFactory, refNameToChromosome);

            var annotatedVariants1 = Annotator.GetAnnotatedVariants(position1.Variants);
            var annotatedVariants2 = Annotator.GetAnnotatedVariants(position2.Variants);
            var annotatedVariants3 = Annotator.GetAnnotatedVariants(position3.Variants);

            // SimplePositions unchanged
            Assert.Equal("<*>", position1.AltAlleles[0]);
            Assert.Equal("*", position2.AltAlleles[0]);
            Assert.Equal("<M>", position3.AltAlleles[0]);

            // Variants are null
            Assert.Null(annotatedVariants1);
            Assert.Null(annotatedVariants2);
            Assert.Null(annotatedVariants3);
        }

        [Fact]
        public void ParseVcfLine_non_informative_alleles_or_NonRef_filtered_only_in_variants()
        {
            const string vcfLine1 = "chr1	13133	.	T	<*>,G	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine2 = "chr1	13133	.	T	*,C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine3 = "chr1	13133	.	T	<M>,A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine4 = "chr1	13133	.	T	A,<NON_REF>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 13133)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var position1 = VcfReaderUtils.ParseVcfLine(vcfLine1, variantFactory, refNameToChromosome);
            var position2 = VcfReaderUtils.ParseVcfLine(vcfLine2, variantFactory, refNameToChromosome);
            var position3 = VcfReaderUtils.ParseVcfLine(vcfLine3, variantFactory, refNameToChromosome);
            var position4 = VcfReaderUtils.ParseVcfLine(vcfLine4, variantFactory, refNameToChromosome);

            var annotatedVariants1 = Annotator.GetAnnotatedVariants(position1.Variants);
            var annotatedVariants2 = Annotator.GetAnnotatedVariants(position2.Variants);
            var annotatedVariants3 = Annotator.GetAnnotatedVariants(position3.Variants);
            var annotatedVariants4 = Annotator.GetAnnotatedVariants(position4.Variants);

            // SimplePositions
            Assert.Equal(new[] { "<*>", "G" }, position1.AltAlleles);
            Assert.Equal(new[] { "*", "C" }, position2.AltAlleles);
            Assert.Equal(new[] { "<M>", "A" }, position3.AltAlleles);
            Assert.Equal(new[] { "A", "<NON_REF>" }, position4.AltAlleles);

            // Variants
            Assert.Equal(new[] { "G" }, annotatedVariants1.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "C" }, annotatedVariants2.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "A" }, annotatedVariants3.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "A" }, annotatedVariants4.Select(x => x.Variant.AltAllele).ToArray());
        }


        [Fact]
        public void Test_crash_caused_by_variant_trimming ()
        {
            const string vcfLine1 = "chr1	8021910	rs373653682	GGTGCTGGACGGTGTCCCT	G	.	.	.";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 8021910)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);
            var position1 = VcfReaderUtils.ParseVcfLine(vcfLine1, variantFactory, refNameToChromosome);

            var annotatedVariants1 = Annotator.GetAnnotatedVariants(position1.Variants);

            // SimplePositions
            Assert.Equal(new[] { "G"}, position1.AltAlleles);

            // Variants
            Assert.Equal(new[] { "" }, annotatedVariants1.Select(x => x.Variant.AltAllele).ToArray());
        }


        [Fact]
        public void ParseVcfLine_line_with_only_NonRef_is_refMinor()
        {
            const string vcfLine = "1	10628385	.	C	<NON_REF>	.	LowGQX;HighDPFRatio	END=10628385;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:24:9:18";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 10628385)).Returns(true);
            refMinorProvider.Setup(x => x.GetGlobalMajorAlleleForRefMinor(chromosome, 10628385)).Returns("T");
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var position = VcfReaderUtils.ParseVcfLine(vcfLine, variantFactory, refNameToChromosome);
            var annotatedVariants = Annotator.GetAnnotatedVariants(position.Variants);

            Assert.Equal("C", position.RefAllele);
            Assert.Equal(new[] { "<NON_REF>" }, position.AltAlleles);
            Assert.Equal("T", position.Variants[0].RefAllele);
            Assert.Equal("C", position.Variants[0].AltAllele);

            // Variants
            Assert.Equal(new[] { "C" }, annotatedVariants.Select(x => x.Variant.AltAllele).ToArray());
        }

        [Fact]
        public void ParseVcfLine_line_with_only_NonRef_is_not_refMinor()
        {
            const string vcfLine = "1	10005	.	C	<NON_REF>	.	LowGQX	END=10034;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:3:1:0";
            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.IsReferenceMinor(chromosome, 10005)).Returns(false);
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["1"] = chromosome };
            var variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider.Object, false);

            var position = VcfReaderUtils.ParseVcfLine(vcfLine, variantFactory, refNameToChromosome);
            var annotatedVariants = Annotator.GetAnnotatedVariants(position.Variants);

            Assert.Equal("C", position.RefAllele);
            Assert.Equal(new[] { "<NON_REF>" }, position.AltAlleles);
            Assert.Equal("C", position.Variants[0].RefAllele);
            Assert.Equal("<NON_REF>", position.Variants[0].AltAllele);

            // Variants
            Assert.Null(annotatedVariants);
        }
    }
}