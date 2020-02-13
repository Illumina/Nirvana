using System.Collections.Generic;
using Genome;

namespace UnitTests.TestUtilities
{
    public static class ChromosomeUtilities
    {
        public static readonly IChromosome Chr1  = new Chromosome("chr1", "1", "", "", 1, 0);
        public static readonly IChromosome Chr2  = new Chromosome("chr2", "2", "", "", 1, 1);
        public static readonly IChromosome Chr3  = new Chromosome("chr3", "3", "", "", 1, 2);
        public static readonly IChromosome Chr4  = new Chromosome("chr4", "4", "", "", 1, 3);
        public static readonly IChromosome Chr5  = new Chromosome("chr5", "5", "", "", 1, 4);
        public static readonly IChromosome Chr6  = new Chromosome("chr6", "6", "", "", 1, 5);
        public static readonly IChromosome Chr7  = new Chromosome("chr7", "7", "", "", 1, 6);
        public static readonly IChromosome Chr8  = new Chromosome("chr8", "8", "", "", 1, 7);
        public static readonly IChromosome Chr9  = new Chromosome("chr9", "9", "", "", 1, 8);
        public static readonly IChromosome Chr10 = new Chromosome("chr10", "10", "", "", 1, 9);
        public static readonly IChromosome Chr11 = new Chromosome("chr11", "11", "", "", 1, 10);
        public static readonly IChromosome Chr12 = new Chromosome("chr12", "12", "", "", 1, 11);
        public static readonly IChromosome Chr13 = new Chromosome("chr13", "13", "", "", 1, 12);
        public static readonly IChromosome Chr14 = new Chromosome("chr14", "14", "", "", 1, 13);
        public static readonly IChromosome Chr15 = new Chromosome("chr15", "15", "", "", 1, 14);
        public static readonly IChromosome Chr16 = new Chromosome("chr16", "16", "", "", 1, 15);
        public static readonly IChromosome Chr17 = new Chromosome("chr17", "17", "", "", 1, 16);
        public static readonly IChromosome Chr18 = new Chromosome("chr18", "18", "", "", 1, 17);
        public static readonly IChromosome Chr19 = new Chromosome("chr19", "19", "", "", 1, 18);
        public static readonly IChromosome Chr20 = new Chromosome("chr20", "20", "", "", 1, 19);
        public static readonly IChromosome Chr21 = new Chromosome("chr21", "21", "", "", 1, 20);
        public static readonly IChromosome Chr22 = new Chromosome("chr22", "22", "", "", 1, 21);
        public static readonly IChromosome ChrX  = new Chromosome("chrX", "X", "", "", 1, 22);
        public static readonly IChromosome ChrY  = new Chromosome("chrY", "Y", "", "", 1, 23);
        public static readonly IChromosome ChrM  = new Chromosome("chrM", "MT", "", "", 1, 24);

        public static readonly IChromosome Bob = new Chromosome("bob", "bob", "", "", 1, Chromosome.UnknownReferenceIndex);

        public static readonly IDictionary<string, IChromosome> RefNameToChromosome = new Dictionary<string, IChromosome>();
        public static readonly Dictionary<ushort, IChromosome> RefIndexToChromosome = new Dictionary<ushort, IChromosome>();

        static ChromosomeUtilities()
        {
            IChromosome[] chromosomes =
            {
                Chr1, Chr2, Chr3, Chr4, Chr5, Chr6, Chr7, Chr8, Chr9, Chr10, Chr11, Chr12, Chr13, Chr14, Chr15, Chr16,
                Chr17, Chr18, Chr19, Chr20, Chr21, Chr22, ChrX, ChrY, ChrM
            };
            foreach (var chromosome in chromosomes) AddChromosome(chromosome);
        }

        private static void AddChromosome(IChromosome chromosome)
        {
            RefIndexToChromosome[chromosome.Index]      = chromosome;
            RefNameToChromosome[chromosome.EnsemblName] = chromosome;
            RefNameToChromosome[chromosome.UcscName]    = chromosome;
        }
    }
}