using System.Collections.Generic;
using Genome;
using Intervals;
using RepeatExpansions.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace RepeatExpansions
{
    public sealed class RepeatExpansionProvider : IRepeatExpansionProvider
    {
        private readonly Matcher _matcher;

        public RepeatExpansionProvider(GenomeAssembly genomeAssembly, IDictionary<string, IChromosome> refNameToChromosome, int numRefSeqs)
        {
            IIntervalForest<RepeatExpansionPhenotype> phenotypeForest = RepeatExpansionReader.Load(genomeAssembly, refNameToChromosome, numRefSeqs);
            _matcher = new Matcher(phenotypeForest);
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            foreach (var variant in annotatedPosition.AnnotatedVariants)
            {
                if (variant.Variant.Type != VariantType.short_tandem_repeat_variation) continue;
                var repeatExpansion = (RepeatExpansion)variant.Variant;

                var phenotypes = _matcher.GetMatchingAnnotations(repeatExpansion);
                if (phenotypes == null) continue;

                variant.RepeatExpansionPhenotypes = phenotypes;
            }
        }
    }
}
