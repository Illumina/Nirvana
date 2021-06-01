using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.GeneFusions.Utilities;

namespace SAUtils.FusionCatcher
{
    public static class FusionCatcherDataSource
    {
        public static void Parse(Stream stream, GeneFusionSource source, CollectionType collectionType,
            Dictionary<ulong, GeneFusionSourceBuilder> geneKeyToFusion, HashSet<string> knownEnsemblGenes)
        {
            Console.Write($"- parsing {source}... ");

            using StreamReader reader              = new StreamReader(stream);
            var                numGeneFusionsAdded = 0;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                string[] cols = line.Split('\t');
                if (cols.Length != 2) throw new InvalidDataException($"Expected 2 columns in the FusionCatcher file, but found {cols.Length}");

                string gene  = cols[0];
                string gene2 = cols[1];

                bool hasGene  = knownEnsemblGenes.Contains(gene);
                bool hasGene2 = knownEnsemblGenes.Contains(gene2);
                if (!hasGene || !hasGene2) continue;

                ulong key = GeneFusionKey.Create(cols[0], cols[1]);

                if (!geneKeyToFusion.TryGetValue(key, out GeneFusionSourceBuilder geneFusion))
                {
                    geneFusion           = new GeneFusionSourceBuilder();
                    geneKeyToFusion[key] = geneFusion;
                }

                switch (collectionType)
                {
                    case CollectionType.Germline:
                        geneFusion.GermlineSources.Add(source);
                        break;
                    case CollectionType.Somatic:
                        geneFusion.SomaticSources.Add(source);
                        break;
                    case CollectionType.Relationships:
                        geneFusion.Relationships.Add(source);
                        break;
                    default:
                        throw new NotSupportedException($"Found an unsupported gene fusion collection type: {collectionType}");
                }

                numGeneFusionsAdded++;
            }

            Console.WriteLine($"added {numGeneFusionsAdded:N0} gene fusions.");
        }
    }
}