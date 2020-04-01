using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Genome;
using Intervals;
using IO;
using OptimizedCore;

namespace RepeatExpansions.IO
{
    public static class RepeatExpansionReader
    {
        private const int ChromIndex          = 0;
        private const int StartIndex          = 1;
        private const int EndIndex            = 2;
        private const int PhenotypeIndex      = 3;
        private const int OmimIndex           = 4;
        private const int RepeatNumbersIndex  = 5;
        private const int AlleleCountsIndex   = 6;
        private const int CategoriesIndex     = 7;
        private const int CategoryRangesIndex = 8;
        private const int MinNumberOfColumns  = 9;

        public static IIntervalForest<RepeatExpansionPhenotype> Load(Stream stream, GenomeAssembly desiredGenomeAssembly,
            IDictionary<string, IChromosome> refNameToChromosome, int numRefSeqs)
        {
            var intervalLists = new List<Interval<RepeatExpansionPhenotype>>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<Interval<RepeatExpansionPhenotype>>();

            

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    CheckHeader(reader, desiredGenomeAssembly);

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;

                        (ushort refIndex, Interval<RepeatExpansionPhenotype> phenotypeInterval) = GetPhenotype(line, refNameToChromosome);
                        if(refIndex == ushort.MaxValue) throw new InvalidDataException("Unknown chromosome encountered in line:\n"+line);
                        intervalLists[refIndex].Add(phenotypeInterval);
                    }
                }
            }

            var refIntervalArrays = new IntervalArray<RepeatExpansionPhenotype>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                refIntervalArrays[i] = new IntervalArray<RepeatExpansionPhenotype>(intervalLists[i].ToArray());
            }

            return new IntervalForest<RepeatExpansionPhenotype>(refIntervalArrays);
        }

        private static (ushort RefIndex, Interval<RepeatExpansionPhenotype> Interval) GetPhenotype(string line, IDictionary<string, IChromosome> refNameToChromosome)
        {
            string[] cols = line.OptimizedSplit('\t');
            if (cols.Length < MinNumberOfColumns) throw new InvalidDataException($"Expected at least {MinNumberOfColumns} columns in the STR data file, but found only {cols.Length}.");

            string chromosomeString         = cols[ChromIndex];
            int start                       = int.Parse(cols[StartIndex]);
            int end                         = int.Parse(cols[EndIndex]);
            string phenotype                = cols[PhenotypeIndex];
            string omimId                   = cols[OmimIndex];
            int[] repeatNumbers             = cols[RepeatNumbersIndex].Split(',').Select(int.Parse).ToArray();
            int[] alleleCounts              = cols[AlleleCountsIndex].Split(',').Select(int.Parse).ToArray();
            string[] classifications        = cols[CategoriesIndex].Split(',').ToArray();
            Interval[] classificationRanges = cols[CategoryRangesIndex].Split(',').Select(GetInterval).ToArray();

            if (repeatNumbers.Length   != alleleCounts.Length)         throw new InvalidDataException($"Inconsistent number of repeat numbers ({repeatNumbers.Length}) vs. allele counts ({alleleCounts.Length})");
            if (classifications.Length != classificationRanges.Length) throw new InvalidDataException($"Inconsistent number of values of classifications ({classifications.Length}) vs. classification ranges ({classificationRanges.Length})");

            var chromosome         = ReferenceNameUtilities.GetChromosome(refNameToChromosome, chromosomeString);
            var chromosomeInterval = new ChromosomeInterval(chromosome, start, end);
            double[] percentiles   = ComputePercentiles(repeatNumbers, alleleCounts);

            var rePhenotype = new RepeatExpansionPhenotype(chromosomeInterval, phenotype, omimId, repeatNumbers, percentiles, classifications, classificationRanges);
            return (chromosome.Index, new Interval<RepeatExpansionPhenotype>(start, end, rePhenotype));
        }

        private static Interval GetInterval(string s)
        {
            string[] cols = s.OptimizedSplit('-');
            int begin     = cols[0] == "inf" ? int.MaxValue : int.Parse(cols[0]);
            int end       = cols[1] == "inf" ? int.MaxValue : int.Parse(cols[1]);

            return new Interval(begin, end);
        }

        private static void CheckHeader(TextReader reader, GenomeAssembly desiredGenomeAssembly)
        {
            string line = reader.ReadLine();
            string genomeAssemblyString = line.OptimizedSplit('=')[1];

            var genomeAssembly = GenomeAssemblyHelper.Convert(genomeAssemblyString);
            if (genomeAssembly != desiredGenomeAssembly) throw new InvalidDataException($"Expected {desiredGenomeAssembly} in the STR data file, but found {genomeAssembly}");

            // skip the header fields line
            reader.ReadLine();
        }

        internal static double[] ComputePercentiles(IReadOnlyCollection<int> repeatNumbers, IReadOnlyList<int> alleleCounts)
        {
            var percentiles       = new double[repeatNumbers.Count];
            var smallerValueCount = 0;
            int totalCount        = alleleCounts.Sum();

            percentiles[0] = 0;
            
            for (var i = 1; i < repeatNumbers.Count; i++)
            {
                smallerValueCount += alleleCounts[i - 1];
                percentiles[i] = 100.0 * smallerValueCount / totalCount;
            }

            return percentiles;
        }
    }
}