using System.Collections.Generic;
using Genome;
using IO;
using VariantAnnotation.Providers;

namespace SAUtils.CosmicGeneFusions.Cache
{
    public static class ReferenceLoader
    {
        public static IDictionary<ushort, IChromosome> GetRefIndexToChromosome(string referencePath)
        {
            var sequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(referencePath));
            return sequenceProvider.RefIndexToChromosome;
        }
    }
}