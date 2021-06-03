using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;

namespace SAUtils.FusionCatcher
{
    public static class IndexBuilder
    {
        public static (GeneFusionSourceCollection[] Index, GeneFusionIndexEntry[] IndexEntries) Convert(Dictionary<ulong, GeneFusionSourceBuilder> geneKeyToSourceBuilder)
        {
            Dictionary<ulong, GeneFusionSourceCollection> geneKeyToSourceCollection = GetSourceCollection(geneKeyToSourceBuilder);

            (GeneFusionSourceCollection[] index, Dictionary<GeneFusionSourceCollection, ushort> sourceCollectionToIndex) =
                BuildIndex(geneKeyToSourceCollection.Values);

            GeneFusionIndexEntry[] indexEntries = BuildIndexEntries(geneKeyToSourceCollection, sourceCollectionToIndex);

            return (index, indexEntries);
        }

        private static GeneFusionIndexEntry[] BuildIndexEntries(Dictionary<ulong, GeneFusionSourceCollection> geneKeyToSourceCollection,
            IReadOnlyDictionary<GeneFusionSourceCollection, ushort> sourceCollectionToIndex)
        {
            var indexEntries = new GeneFusionIndexEntry[geneKeyToSourceCollection.Count];
            var currentIndex = 0;

            foreach ((ulong geneKey, GeneFusionSourceCollection sourceCollection) in geneKeyToSourceCollection.OrderBy(x => x.Key))
            {
                if (!sourceCollectionToIndex.TryGetValue(sourceCollection, out ushort index))
                    throw new InvalidDataException($"Unable to find the gene fusion source collection for gene key: {geneKey}");

                indexEntries[currentIndex++] = new GeneFusionIndexEntry(geneKey, index);
            }

            return indexEntries;
        }

        private static (GeneFusionSourceCollection[] Index, Dictionary<GeneFusionSourceCollection, ushort> SourceCollectionToIndex) BuildIndex(
            Dictionary<ulong, GeneFusionSourceCollection>.ValueCollection sourceCollections)
        {
            var collectionToHits = new Dictionary<GeneFusionSourceCollection, BuilderMetadata>();

            foreach (GeneFusionSourceCollection sourceCollection in sourceCollections)
            {
                if (collectionToHits.TryGetValue(sourceCollection, out BuilderMetadata metadata))
                {
                    metadata.NumHits++;
                }
                else
                {
                    collectionToHits[sourceCollection] = new BuilderMetadata {NumHits = 1, SourceCollection = sourceCollection};
                }
            }

            // we want to order these in descending popularity
            BuilderMetadata[] sortedIndex             = collectionToHits.Values.OrderByDescending(x => x.NumHits).ToArray();
            var               index                   = new GeneFusionSourceCollection[sortedIndex.Length];
            var               sourceCollectionToIndex = new Dictionary<GeneFusionSourceCollection, ushort>();

            for (var i = 0; i < sortedIndex.Length; i++)
            {
                GeneFusionSourceCollection sourceCollection = sortedIndex[i].SourceCollection;
                index[i]                                  = sourceCollection;
                sourceCollectionToIndex[sourceCollection] = (ushort) i;
            }

            return (index, sourceCollectionToIndex);
        }

        private static Dictionary<ulong, GeneFusionSourceCollection> GetSourceCollection(
            Dictionary<ulong, GeneFusionSourceBuilder> geneKeyToSourceBuilder)
        {
            var geneKeyToSourceCollection = new Dictionary<ulong, GeneFusionSourceCollection>(geneKeyToSourceBuilder.Count);

            foreach ((ulong geneKey, GeneFusionSourceBuilder builder) in geneKeyToSourceBuilder)
            {
                GeneFusionSourceCollection sourceCollection = builder.Create();
                geneKeyToSourceCollection[geneKey] = sourceCollection;
            }

            return geneKeyToSourceCollection;
        }

        private sealed class BuilderMetadata
        {
            public int                        NumHits;
            public GeneFusionSourceCollection SourceCollection;
        }
    }
}