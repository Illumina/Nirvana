using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.GeneFusions.Utilities;

namespace SAUtils.FusionCatcher
{
    public static class FusionCatcherOncogenes
    {
        public static void Parse(Stream stream, string description, HashSet<uint> oncoGenes, HashSet<string> knownEnsemblGenes)
        {
            Console.Write($"- parsing {description} oncogenes... ");

            using var reader            = new StreamReader(stream);
            var       numOncogenesAdded = 0;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                string[] cols = line.Split('\t');
                if (cols.Length != 1) throw new InvalidDataException($"Expected 1 column in the FusionCatcher file, but found {cols.Length}");

                string gene = cols[0];

                bool hasGene = knownEnsemblGenes.Contains(gene);
                if (!hasGene) continue;

                uint geneKey = GeneFusionKey.CreateGeneKey(gene);
                oncoGenes.Add(geneKey);

                numOncogenesAdded++;
            }

            Console.WriteLine($"added {numOncogenesAdded:N0} oncogenes.");
        }
    }
}