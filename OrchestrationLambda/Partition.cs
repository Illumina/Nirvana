using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Genome;
using Intervals;

namespace OrchestrationLambda
{
    public static class Partition
    {
        // max sizes for whole file processing, split by chr and split by arm, respectively
        private static readonly long[] VcfSizeBreaks = { 100_000, 1_000_000, 1_900_000_000 };
        private static readonly long[] GvcfSizeBreaks = { 200_000, 2_000_000, 3_900_000_000 };

        internal static PatitionStrategy GetStrategy(long fileSize, bool isGvcf)
        {
            var sizeBreaks = isGvcf ? GvcfSizeBreaks : VcfSizeBreaks;
            if (fileSize <= sizeBreaks[0])
                return PatitionStrategy.WholeVcf;
            if (fileSize <= sizeBreaks[1])
                return PatitionStrategy.ByChr;
            if (fileSize <= sizeBreaks[2])
                return PatitionStrategy.ByArm;
            throw new ArgumentOutOfRangeException($"The VCF file is too large to process: {fileSize} bytes");
        }

        public static IEnumerable<IChromosomeInterval> GetChromosomeIntervals(long fileSize, bool isGvcf, GenomeAssembly genomeAssembly, IEnumerable<IChromosome> chromosomes)
        {
            var partitionStrategy = GetStrategy(fileSize, isGvcf);

            switch (partitionStrategy)
            {
                case PatitionStrategy.WholeVcf:
                    return null;
                case PatitionStrategy.ByChr:
                    return chromosomes.Select(GetWholeChromosomeInterval);
                case PatitionStrategy.ByArm:
                    return chromosomes.Select(x => GetThisChromosomeIntervals(x, PartitionInfo.Intervals[genomeAssembly])).SelectMany(x => x);
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported partition strategy: {partitionStrategy}");
            }
        }

        private static IChromosomeInterval GetWholeChromosomeInterval(IChromosome chromosome) => new ChromosomeInterval(chromosome, 1, int.MaxValue);

        private static IEnumerable<IChromosomeInterval> GetThisChromosomeIntervals(IChromosome chromosome, ImmutableDictionary<string, IInterval[]> partitionInfo)
        {
            if (partitionInfo.TryGetValue(chromosome.UcscName, out var intervals))
            {
                return intervals.Select(x => new ChromosomeInterval(chromosome, x.Start, x.End));
            }
            return new[] {GetWholeChromosomeInterval(chromosome)};
        }
    }

}