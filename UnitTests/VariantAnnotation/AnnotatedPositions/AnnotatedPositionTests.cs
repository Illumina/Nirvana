using Moq;
using UnitTests.SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;
using Vcf;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedPositionTests
    {
        [Fact]
        public void GetJsonString_DifferentOriginalChromosomeName()
        {
            const string originalChromosomeName = "originalChr1";

            IVariant[] variants = GetVariants();
            ISample[] samples   = GetSamples();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);

            var position          = GetPosition(originalChromosomeName, variants, samples);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            string observedResult = annotatedPosition.GetJsonString();

            Assert.NotNull(observedResult);
            Assert.Contains($"\"chromosome\":\"{originalChromosomeName}\"", observedResult);
        }

        [Fact]
        public void GetJsonString_NullAnnotatedVariants()
        {
            const string originalChromosomeName = "originalChr1";

            var position = GetPosition(originalChromosomeName, null, null);
            var annotatedPosition = new AnnotatedPosition(position, null);

            string observedResult = annotatedPosition.GetJsonString();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetJsonString_StrelkaSomatic()
        {
            const string vcfLine = "chr1	13813	.	T	G	.	LowQscore	SOMATIC;QSS=33;TQSS=1;NT=ref;QSS_NT=16;TQSS_NT=1;SGT=TT->GT;DP=266;MQ=23.89;MQ0=59;ALTPOS=69;ALTMAP=37;ReadPosRankSum=1.22;SNVSB=5.92;PNOISE=0.00;PNOISE2=0.00;VQSR=1.93";

            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13813, "T", 'C', ChromosomeUtilities.RefNameToChromosome);
            var variantFactory = new VariantFactory(seqProvider.Sequence, new VariantId());

            var position = AnnotationUtilities.ParseVcfLine(vcfLine, refMinorProvider.Object, seqProvider, variantFactory);

            IVariant[] variants = GetVariants();
            IAnnotatedVariant[] annotatedVariants = Annotator.GetAnnotatedVariants(variants);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            string observedResult = annotatedPosition.GetJsonString();

            Assert.NotNull(observedResult);
            Assert.Contains("\"jointSomaticNormalQuality\":16", observedResult);
            Assert.Contains("\"recalibratedQuality\":1.93", observedResult);
        }

        private static ISample[] GetSamples() => new ISample[] { Sample.EmptySample };

        private static IVariant[] GetVariants()
        {
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(ChromosomeUtilities.Chr1);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(949523);
            variant.SetupGet(x => x.End).Returns(949523);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(AnnotationBehavior.SmallVariants);
            return new[] { variant.Object };
        }

        private static IPosition GetPosition(string originalChromosomeName, IVariant[] variants, ISample[] samples)
        {
            var vcfFields = new string[8];
            vcfFields[0] = originalChromosomeName;

            var infoData = new InfoData(null, null, null, null, null, null, null, null, null, null);

            return new Position(ChromosomeUtilities.Chr1, 949523, 949523, "C", new[] {"T"}, null, null, variants, samples, infoData,
                vcfFields, new[] { false }, false);
        }
    }
}
