using System.Collections.Generic;
using System.Linq;
using Genome;
using Moq;
using UnitTests.SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.Interface.Providers;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfReaderUtilsTests
    {
#if (NI_ALLELE)
        [Fact]
        public void ParseVcfLine_NonInformativeAlleles_Alone_NotFiltered()
        {
            const string vcfLine1 = "chr1	13133	.	T	<*>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine2 = "chr1	13133	.	T	*	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine3 = "chr1	13133	.	T	<M>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome          = new Chromosome("chr1", "1", 0);
            var refMinorProvider    = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13133, "T", 'A',
                new Dictionary<string, IChromosome> {["chr1"] = chromosome});
            var variantFactory      = new VariantFactory(seqProvider);

            var position1 = AnnotationUtilities.ParseVcfLine(vcfLine1, refMinorProvider.Object, variantFactory, seqProvider.RefNameToChromosome);
            var position2 = AnnotationUtilities.ParseVcfLine(vcfLine2, refMinorProvider.Object, variantFactory, seqProvider.RefNameToChromosome);
            var position3 = AnnotationUtilities.ParseVcfLine(vcfLine3, refMinorProvider.Object, variantFactory, seqProvider.RefNameToChromosome);

            var annotatedVariants1 = Annotator.GetAnnotatedVariants(position1.Variants);
            var annotatedVariants2 = Annotator.GetAnnotatedVariants(position2.Variants);
            var annotatedVariants3 = Annotator.GetAnnotatedVariants(position3.Variants);

            // SimplePositions unchanged
            Assert.Equal("<*>", position1.AltAlleles[0]);
            Assert.Equal("*", position2.AltAlleles[0]);
            Assert.Equal("<M>", position3.AltAlleles[0]);

            // Variants not filtered
            Assert.Equal("<*>", annotatedVariants1[0].Variant.AltAllele);
            Assert.Equal("*", annotatedVariants2[0].Variant.AltAllele);
            Assert.Equal("<M>", annotatedVariants3[0].Variant.AltAllele);
        }

        [Fact]
        public void ParseVcfLine_NonInformativeAlleles_WithNormalAllele_NotFiltered()
        {
            const string vcfLine1 = "chr1	13133	.	T	<*>,G	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine2 = "chr1	13133	.	T	*,C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine3 = "chr1	13133	.	T	<M>,A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine4 = "chr1	13133	.	T	A,<NON_REF>	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";

            var chromosome = new Chromosome("chr1", "1", 0);
            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13133, "T", 'A',
                new Dictionary<string, IChromosome> { ["chr1"] = chromosome });
            var refNameToChromosome = seqProvider.RefNameToChromosome;

            var variantFactory = new VariantFactory(seqProvider);

            var position1 = AnnotationUtilities.ParseVcfLine(vcfLine1, refMinorProvider.Object, variantFactory, refNameToChromosome);
            var position2 = AnnotationUtilities.ParseVcfLine(vcfLine2, refMinorProvider.Object, variantFactory, refNameToChromosome);
            var position3 = AnnotationUtilities.ParseVcfLine(vcfLine3, refMinorProvider.Object, variantFactory, refNameToChromosome);
            var position4 = AnnotationUtilities.ParseVcfLine(vcfLine4, refMinorProvider.Object, variantFactory, refNameToChromosome);

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
            Assert.Equal(new[] { "<*>", "G" }, annotatedVariants1.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "*", "C" }, annotatedVariants2.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "<M>", "A" }, annotatedVariants3.Select(x => x.Variant.AltAllele).ToArray());
            Assert.Equal(new[] { "A", "<NON_REF>" }, annotatedVariants4.Select(x => x.Variant.AltAllele).ToArray());
        }
#endif

        [Fact]
        public void Test_crash_caused_by_variant_trimming ()
        {
            const string vcfLine1 = "chr1	8021910	rs373653682	GGTGCTGGACGGTGTCCCT	G	.	.	.";

            var chromosome          = new Chromosome("chr1", "1", 0);
            var refMinorProvider    = new Mock<IRefMinorProvider>();
            var seqProvider         = ParserTestUtils.GetSequenceProvider(8021910, "GGTGCTGGACGGTGTCCCT", 'A',
                new Dictionary<string, IChromosome> { ["chr1"] = chromosome });
            var refNameToChromosome = seqProvider.RefNameToChromosome;

            var variantFactory = new VariantFactory(seqProvider);

            var position1           = AnnotationUtilities.ParseVcfLine(vcfLine1, refMinorProvider.Object, variantFactory, refNameToChromosome);

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

            var chromosome           = new Chromosome("chr1", "1", 0);
            var refMinorProvider     = new Mock<IRefMinorProvider>();
            refMinorProvider.Setup(x => x.GetGlobalMajorAllele(chromosome, 10628385)).Returns("T");
            var seqProvider = ParserTestUtils.GetSequenceProvider(10628385, "C", 'A',
                new Dictionary<string, IChromosome> { ["1"] = chromosome });
            var refNameToChromosome = seqProvider.RefNameToChromosome;

            var variantFactory = new VariantFactory(seqProvider);


            var position          = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, variantFactory, refNameToChromosome);
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

            var chromosome          = new Chromosome("chr1", "1", 0);
            var refMinorProvider    = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(10005, "C", 'A',
                new Dictionary<string, IChromosome> { ["1"] = chromosome });
            var refNameToChromosome = seqProvider.RefNameToChromosome;

            var variantFactory = new VariantFactory(seqProvider);

            var position          = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, variantFactory, refNameToChromosome);
            var annotatedVariants = Annotator.GetAnnotatedVariants(position.Variants);

            Assert.Equal("C", position.RefAllele);
            Assert.Equal(new[] { "<NON_REF>" }, position.AltAlleles);
            Assert.Null(position.Variants);
            Assert.Null(annotatedVariants);
        }
    }
}