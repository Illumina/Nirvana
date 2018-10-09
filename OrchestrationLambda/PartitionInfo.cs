using System.Collections.Generic;
using System.Collections.Immutable;
using Genome;
using Intervals;

namespace OrchestrationLambda
{
    public static class PartitionInfo
    {
        private static readonly List<ChromosomePartition> Grch37Partitions =
            new List<ChromosomePartition>
        {
            new ChromosomePartition("chr1", "1", 0, new[] {29953082, 103888906, 149484645, 205997707, 249250621}),
            new ChromosomePartition("chr2", "2", 1, new[] {33142192, 93826171, 149740582, 243199373}),
            new ChromosomePartition("chr3", "3", 2, new[] {60104769, 92004854, 198022430}),
            new ChromosomePartition("chr4", "4", 3, new[] {40296746, 75439829, 191154276}),
            new ChromosomePartition("chr5", "5", 4, new[] {47905641, 91661128, 138812073, 180915260}),
            new ChromosomePartition("chr6", "6", 5, new[] {58112659, 95755543, 171115067}),
            new ChromosomePartition("chr7", "7", 6, new[] {50390631, 50390631, 130204523, 159138663}),
            new ChromosomePartition("chr8", "8", 7, new[] {45338887, 86651451, 146364022}),
            new ChromosomePartition("chr9", "9", 8, new[] {39688686, 56392679, 92393416, 141213431}),
            new ChromosomePartition("chr10", "10", 9, new[] {54900229, 116065825, 135534747}),
            new ChromosomePartition("chr11", "11", 10, new[] {50937353, 96362584, 135006516}),
            new ChromosomePartition("chr12", "12", 11, new[] {36356694, 109398470, 133851895}),
            new ChromosomePartition("chr13", "13", 12, new[] {86835324, 115169878}),
            new ChromosomePartition("chr14", "14", 13, new[] {9500000, 107349540}),
            new ChromosomePartition("chr15", "15", 13, new[] {29184443, 102531392}),
            new ChromosomePartition("chr16", "16", 13, new[] {40835801, 90354753}),
            new ChromosomePartition("chrX", "X", 22, new[] {37123256, 76678692, 120038235, 155270560})
        };

        private static readonly List<ChromosomePartition> Grch38Partitions =
            new List<ChromosomePartition>
            {
                new ChromosomePartition("chr1", "1", 0, new[] {124785482, 248956422}),
                new ChromosomePartition("chr2", "2", 1, new[] {109493618, 242193529}),
                new ChromosomePartition("chr3", "3", 2, new[] {93680574, 198295559}),
                new ChromosomePartition("chr4", "4", 3, new[] {58900087, 190214555}),
                new ChromosomePartition("chr5", "5", 4, new[] {50084807, 181538259}),
                new ChromosomePartition("chr6", "6", 5, new[] {95045790, 170805979}),
                new ChromosomePartition("chr7", "7", 6, new[] {62481779, 159345973}),
                new ChromosomePartition("chr8", "8", 7, new[] {85689222, 145138636}),
                new ChromosomePartition("chr9", "9", 8, new[] {68070552, 138394717}),
                new ChromosomePartition("chr10", "10", 9, new[] {62580397, 133797422}),
                new ChromosomePartition("chr11", "11", 10, new[] {71005696, 135086622}),
                new ChromosomePartition("chr12", "12", 11, new[] {37460080, 133275309}),
                new ChromosomePartition("chr13", "13", 12, new[] {86227979, 114364328}),
                new ChromosomePartition("chr14", "14", 13, new[] {19561713, 107043718}),
                new ChromosomePartition("chrX", "X", 22, new[] {62437542, 156040895})
            };

        public static readonly ImmutableDictionary<GenomeAssembly, ImmutableDictionary<string, IInterval[]>> Intervals =
            new Dictionary<GenomeAssembly, ImmutableDictionary<string, IInterval[]>> {
                {
                    GenomeAssembly.GRCh37, GetChromosomeToPartitions(Grch37Partitions)
                },
                {
                    GenomeAssembly.GRCh38, GetChromosomeToPartitions(Grch38Partitions)
                }
            }.ToImmutableDictionary();


        private static ImmutableDictionary<string, IInterval[]> GetChromosomeToPartitions(List<ChromosomePartition> chromosomePartitions)
        {
            var chrToPartitions = new Dictionary<string, IInterval[]>();
            chromosomePartitions.ForEach(x => AddChrPartitions(x, chrToPartitions));

            return chrToPartitions.ToImmutableDictionary();
        }

        private static void AddChrPartitions(ChromosomePartition chromosomePartition,
            IDictionary<string, IInterval[]> chrToPartitions)
        {
            var numPartitions = chromosomePartition.PartitionEnds.Length;
            var partitions = new IInterval[numPartitions];
            var start = 1;
            for (var i = 0; i < numPartitions; i++)
            {
                var end = chromosomePartition.PartitionEnds[i];
                partitions[i] = new Interval(start, end);
                start = end + 1;
            }
            chrToPartitions.Add(chromosomePartition.UcscName, partitions);
            chrToPartitions.Add(chromosomePartition.EnsemblName, partitions);
        }
    }
}