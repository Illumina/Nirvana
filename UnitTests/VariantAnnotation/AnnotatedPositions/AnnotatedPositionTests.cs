using Genome;
using Moq;
using VariantAnnotation;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using Variants;
using Vcf;
using Vcf.Info;
using Vcf.Sample;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedPositionTests
    {
        private readonly IChromosome _chromosome;

        public AnnotatedPositionTests()
        {
            _chromosome = new Chromosome("chr1", "1", 0);
        }

        [Fact]
        public void GetJsonString_DifferentOriginalChromosomeName()
        {
            const string originalChromosomeName = "originalChr1";

            var variants = GetVariants();
            var samples = GetSamples();
            var annotatedVariants = Annotator.GetAnnotatedVariants(variants);

            var position = GetPosition(originalChromosomeName, variants, samples);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var observedResult = annotatedPosition.GetJsonString();

            Assert.NotNull(observedResult);
            Assert.Contains($"\"chromosome\":\"{originalChromosomeName}\"", observedResult);
        }

        private static ISample[] GetSamples() => new ISample[] { Sample.EmptySample };

        [Fact]
        public void GetJsonString_NullAnnotatedVariants()
        {
            const string originalChromosomeName = "originalChr1";

            var position = GetPosition(originalChromosomeName, null, null);
            var annotatedPosition = new AnnotatedPosition(position, null);

            var observedResult = annotatedPosition.GetJsonString();

            Assert.Null(observedResult);
        }

        private IVariant[] GetVariants()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);
            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Chromosome).Returns(_chromosome);
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(949523);
            variant.SetupGet(x => x.End).Returns(949523);
            variant.SetupGet(x => x.RefAllele).Returns("C");
            variant.SetupGet(x => x.AltAllele).Returns("T");
            variant.SetupGet(x => x.Behavior).Returns(behavior);
            return new[] { variant.Object };
        }

        private IPosition GetPosition(string originalChromosomeName, IVariant[] variants, ISample[] samples)
        {
            var vcfFields = new string[8];
            vcfFields[0] = originalChromosomeName;

            var infoData = new InfoData(null, null, null, null, null, null, null, null, VariantType.unknown);

            return new Position(_chromosome, 949523, 949523, "C", new[] {"T"}, null, null, variants, samples, infoData,
                vcfFields, new[] { false }, false);
        }
    }
}
