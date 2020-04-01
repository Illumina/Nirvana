using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Genome;
using Intervals;
using IO;
using RepeatExpansions.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace RepeatExpansions
{
    public sealed class RepeatExpansionProvider : IRepeatExpansionProvider
    {
        private readonly Matcher _matcher;

        public RepeatExpansionProvider(GenomeAssembly genomeAssembly, IDictionary<string, IChromosome> refNameToChromosome, 
            int numRefSeqs, string customTsvPath)
        {
            using ( Stream stream = GetTsvStream(genomeAssembly, customTsvPath))
            {
                IIntervalForest<RepeatExpansionPhenotype> phenotypeForest = RepeatExpansionReader.Load(stream, genomeAssembly, refNameToChromosome, numRefSeqs);
                _matcher = new Matcher(phenotypeForest);
            }
        }

        private static Stream GetTsvStream(GenomeAssembly genomeAssembly, string customTsvPath)
        {
            //since we are using the executing assembly, we cannot move the following lines about getting stream further upstream.
            var    assembly     = Assembly.GetExecutingAssembly();
            string resourceName = $"RepeatExpansions.Resources.RepeatExpansions.{genomeAssembly}.tsv";
            var stream = customTsvPath != null
                ? PersistentStreamUtils.GetReadStream(customTsvPath)
                : assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null) throw new NullReferenceException("Unable to read from the STR resource file");
            return stream;
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
