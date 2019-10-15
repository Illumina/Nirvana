using System.Collections.Generic;
using Intervals;
using VariantAnnotation.Interface.SA;
using Variants;

namespace RepeatExpansions
{
    public sealed class Matcher
    {
        private readonly IIntervalForest<RepeatExpansionPhenotype> _phenotypeForest;

        public Matcher(IIntervalForest<RepeatExpansionPhenotype> phenotypeForest) => _phenotypeForest = phenotypeForest;

        public ISupplementaryAnnotation GetMatchingAnnotations(RepeatExpansion variant)
        {
            RepeatExpansionPhenotype[] variantPhenotypes =
                _phenotypeForest.GetAllOverlappingValues(variant.Chromosome.Index, variant.Start, variant.End);
            if (variantPhenotypes == null) return null;

            var jsonEntries = new List<string>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var variantPhenotype in variantPhenotypes)
            {
                if (!ExactMatch(variant, variantPhenotype.ChromosomeInterval)) continue;

                string json = variantPhenotype.GetAnnotation(variant.RepeatCount);
                jsonEntries.Add(json);
            }

            return jsonEntries.Count == 0 ? null : new RepeatExpansionSupplementaryAnnotation(jsonEntries);
        }

        private static bool ExactMatch(IInterval variant, IInterval variantPhenotype) =>
            variant.Start == variantPhenotype.Start && 
            variant.End   == variantPhenotype.End;
    }
}
