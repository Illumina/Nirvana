using System.Collections.Generic;
using Genome;
using OptimizedCore;
using RepeatExpansions;
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

        private readonly IChromosome _chr3 = new Chromosome("chr3", "3", 2);
        private const int Start            = 63898361;
        private const int End              = 63898390;

        public RepeatExpansionProviderTests()
        {
            // retire this as soon as we merge the 3.2.2 branch into develop
            var refNameToChromosome = new Dictionary<string, IChromosome>();
            AddReference(refNameToChromosome, "1", "chr1", 0);
            AddReference(refNameToChromosome, "2", "chr2", 1);
            AddReference(refNameToChromosome, "3", "chr3", 2);
            AddReference(refNameToChromosome, "4", "chr4", 3);
            AddReference(refNameToChromosome, "5", "chr5", 4);
            AddReference(refNameToChromosome, "6", "chr5", 5);
            AddReference(refNameToChromosome, "9", "chr9", 8);
            AddReference(refNameToChromosome, "11", "chr11", 10);
            AddReference(refNameToChromosome, "12", "chr12", 11);
            AddReference(refNameToChromosome, "13", "chr13", 12);
            AddReference(refNameToChromosome, "14", "chr14", 13);
            AddReference(refNameToChromosome, "15", "chr15", 14);
            AddReference(refNameToChromosome, "16", "chr16", 15);
            AddReference(refNameToChromosome, "17", "chr17", 16);
            AddReference(refNameToChromosome, "18", "chr18", 17);
            AddReference(refNameToChromosome, "19", "chr19", 18);
            AddReference(refNameToChromosome, "20", "chr20", 19);
            AddReference(refNameToChromosome, "21", "chr21", 20);
            AddReference(refNameToChromosome, "22", "chr22", 21);
            AddReference(refNameToChromosome, "X", "chrX", 22);

            _provider = new RepeatExpansionProvider(GenomeAssembly.GRCh37, refNameToChromosome, 23);
        }

        private static void AddReference(IDictionary<string, IChromosome> refNameToChromosome, string ucscName, string ensemblName, ushort refIndex)
        {
            var chromosome = new Chromosome(ucscName, ensemblName, refIndex);
            refNameToChromosome[ucscName]    = chromosome;
            refNameToChromosome[ensemblName] = chromosome;
        }

        [Fact]
        public void Annotate_NotRepeatExpansion_NullPhenotypes()
        {
            var variant = new Variant(_chr3, Start, End, "A", "C", VariantType.SNV, null, false, false, false, null, 
                AnnotationBehavior.SmallVariants, false);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);
            
            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.Null(firstVariant.RepeatExpansionPhenotypes);
        }

        [Fact]
        public void Annotate_RepeatExpansion_NotExactMatch_NullPhenotypes()
        {
            var variant = new RepeatExpansion(_chr3, Start, End + 1, "A", "<STR3>", null, 10, 5);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);

            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.Null(firstVariant.RepeatExpansionPhenotypes);
        }

        [Fact]
        public void Annotate_RepeatExpansion_no_refRepeatCount()
        {
            var variant = new RepeatExpansion(_chr3, Start, End + 1, "A", "<STR3>", null, 10, null);

            var annotatedPosition = GetAnnotatedPosition(variant);
            _provider.Annotate(annotatedPosition);

            var firstVariant = annotatedPosition.AnnotatedVariants[0];
            Assert.NotNull(firstVariant);
        }

        [Fact]
        public void Annotate_RepeatExpansion_ExactMatch_OnePhenotype()
        {
            var variant = new RepeatExpansion(_chr3, Start, End, "A", "<STR3>", null, 10, 5);

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

        private IAnnotatedPosition GetAnnotatedPosition(IVariant variant)
        {
            IVariant[] variants = { variant };
            var position = new Position(_chr3, Start, End, null, null, null, null, variants, null, null, null, null,
                false);

            var annotatedVariant = new AnnotatedVariant(variant);
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };

            return new AnnotatedPosition(position, annotatedVariants);
        }
    }
}
