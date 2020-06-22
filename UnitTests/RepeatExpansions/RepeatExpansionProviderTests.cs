using Genome;
using OptimizedCore;
using RepeatExpansions;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;
using Variants;
using Vcf;
using Xunit;

namespace UnitTests.RepeatExpansions
{
    public sealed class RepeatExpansionProviderTests
    {
        private readonly RepeatExpansionProvider _provider;

        private const int Start = 63898361;
        private const int End   = 63898390;

        public RepeatExpansionProviderTests()
        {
            _provider = new RepeatExpansionProvider(GenomeAssembly.GRCh37, ChromosomeUtilities.RefNameToChromosome, 23, null);
        }

        
        [Fact]
        public void Annotate_NotRepeatExpansion_NullPhenotypes()
        {
            var variant = new Variant(ChromosomeUtilities.Chr3, Start, End, "A", "C", VariantType.SNV, null, false, false, false, null, 
                AnnotationBehavior.SmallVariants, false);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);
            
            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.Null(firstVariant.RepeatExpansionPhenotypes);
        }

        [Fact]
        public void Annotate_RepeatExpansion_NotExactMatch_NullPhenotypes()
        {
            var variant = new RepeatExpansion(ChromosomeUtilities.Chr3, Start, End + 1, "A", "<STR3>", null, 10, 5);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);

            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.Null(firstVariant.RepeatExpansionPhenotypes);
        }

        [Fact]
        public void Annotate_RepeatExpansion_no_refRepeatCount()
        {
            var variant = new RepeatExpansion(ChromosomeUtilities.Chr3, Start, End + 1, "A", "<STR3>", null, 10, null);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);

            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.NotNull(firstVariant);
        }

        [Fact]
        public void Annotate_RepeatExpansion_ExactMatch_OnePhenotype()
        {
            var variant = new RepeatExpansion(ChromosomeUtilities.Chr3, Start, End, "A", "<STR3>", null, 10, 5);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);

            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.NotNull(firstVariant.RepeatExpansionPhenotypes);

            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddObjectValue(firstVariant.RepeatExpansionPhenotypes.JsonKey,
                firstVariant.RepeatExpansionPhenotypes);

            const string expectedJson = "\"repeatExpansionPhenotypes\":[{\"phenotype\":\"Spinocerebellar ataxia 7\",\"omimId\":164500,\"classifications\":[\"Normal\"],\"percentile\":6.33}]";
            string observedJson = sb.ToString();
            Assert.Equal(expectedJson, observedJson);
        }

        private static IAnnotatedPosition GetAnnotatedPosition(IVariant variant)
        {
            IVariant[] variants = { variant };
            var position = new Position(ChromosomeUtilities.Chr3, Start, End, null, null, null, null, variants, null, null, null, null,
                false);

            var annotatedVariant = new AnnotatedVariant(variant);
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };

            return new AnnotatedPosition(position, annotatedVariants);
        }
    }
}
