using System.Collections.Generic;
using System.IO;
using Genome;

namespace ReferenceSequence.IO
{
    public static class CytogeneticBandsReader
    {
        public static List<Band>[] GetCytogeneticBands(Stream stream, int numRefSeqs, Dictionary<string, Chromosome> refNameToChromosome)
        {
            var bandLists = new List<Band>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) bandLists[i] = new List<Band>();

            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    string[] cols = line.Split('\t');

                    const int expectedNumColumns = 5;

                    if (cols.Length != expectedNumColumns)
                    {
                        throw new InvalidDataException($"Expected {expectedNumColumns} columns, but found {cols.Length} columns: [{line}]");
                    }

                    string ucscName = cols[0];
                    int begin       = int.Parse(cols[1]) + 1;
                    int end         = int.Parse(cols[2]);
                    string name     = cols[3];

                    var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, ucscName);
                    if (chromosome.IsEmpty) continue;

                    bandLists[chromosome.Index].Add(new Band(begin, end, name));
                }
            }

            return bandLists;
        }
    }
}
