using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
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
                        if(line == string.Empty) continue;

                        try
                        {
                            (ushort refIndex, Interval<RepeatExpansionPhenotype> phenotypeInterval) = GetPhenotype(line, refNameToChromosome);
                            if(refIndex == ushort.MaxValue) throw new InvalidDataException("Unknown chromosome encountered in STR file.");
                            intervalLists[refIndex].Add(phenotypeInterval);
                        }
                        catch (Exception e)
                        {
                            e.Data[ExitCodeUtilities.Line] = line;
                            throw;
                        }
                        
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
            double[] percentiles   = PercentileUtilities.ComputePercentiles(repeatNumbers.Length, alleleCounts);

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
            while (line == string.Empty) line = reader.ReadLine();
            if(line==null) throw new UserErrorException("The custom STR file provided is empty.");

            GenomeAssembly genomeAssembly = GenomeAssembly.Unknown;
            var headerNum = 0;
            while (line!=null && line.StartsWith("#"))
            {
                headerNum++;
                line = line.Trim();
                var columns = line.Split('=','\t');
                var tag = columns[0].ToLower();
                switch (headerNum)
                {
                    case 1:
                        if(tag != "#assembly")
                            throw new UserErrorException("First line in STR data file has to contain assembly. For example: #assembly=GRCh38");
                        genomeAssembly = GenomeAssemblyHelper.Convert(columns[1]);
                        if (genomeAssembly != desiredGenomeAssembly) 
                            throw new UserErrorException($"Expected {desiredGenomeAssembly} in the STR data file, but found {genomeAssembly}");
                        break;
                    case 2:
                        if(tag!="#chrom")
                            throw new UserErrorException("Second line in TSV has to contain column labels. For example: #Chrom\tStart\tEnd\tPhenotype\t...");
                        return; // we should not read the next line
                    default:
                        throw new UserErrorException($"Unexpected header tag observed:\n{line}");
                }
                line = reader.ReadLine();
            }
            if(genomeAssembly == GenomeAssembly.Unknown) 
                throw new UserErrorException("Genome assembly not specified in STR header. It is a required field.");

        }
    }
}