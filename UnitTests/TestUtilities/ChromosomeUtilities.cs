using System.Collections.Generic;
using Genome;

namespace UnitTests.TestUtilities;

public static class ChromosomeUtilities
{
    public static readonly Chromosome Chr1  = new("chr1", "1", "NC_000001.10", "", 249250621, 0);
    public static readonly Chromosome Chr2  = new("chr2", "2", "NC_000002.11", "", 243199373, 1);
    public static readonly Chromosome Chr3  = new("chr3", "3", "NC_000003.11", "", 198022430, 2);
    public static readonly Chromosome Chr4  = new("chr4", "4", "NC_000004.11", "", 191154276, 3);
    public static readonly Chromosome Chr5  = new("chr5", "5", "NC_000005.9", "", 180915260, 4);
    public static readonly Chromosome Chr6  = new("chr6", "6", "NC_000006.11", "", 171115067, 5);
    public static readonly Chromosome Chr7  = new("chr7", "7", "NC_000007.13", "", 159138663, 6);
    public static readonly Chromosome Chr8  = new("chr8", "8", "NC_000008.10", "", 146364022, 7);
    public static readonly Chromosome Chr9  = new("chr9", "9", "NC_000009.11", "", 141213431, 8);
    public static readonly Chromosome Chr10 = new("chr10", "10", "NC_000010.10", "", 135534747, 9);
    public static readonly Chromosome Chr11 = new("chr11", "11", "NC_000011.9", "", 135006516, 10);
    public static readonly Chromosome Chr12 = new("chr12", "12", "NC_000012.11", "", 133851895, 11);
    public static readonly Chromosome Chr13 = new("chr13", "13", "NC_000013.10", "", 115169878, 12);
    public static readonly Chromosome Chr14 = new("chr14", "14", "NC_000014.8", "", 107349540, 13);
    public static readonly Chromosome Chr15 = new("chr15", "15", "NC_000015.9", "", 102531392, 14);
    public static readonly Chromosome Chr16 = new("chr16", "16", "NC_000016.9", "", 90354753, 15);
    public static readonly Chromosome Chr17 = new("chr17", "17", "NC_000017.10", "", 81195210, 16);
    public static readonly Chromosome Chr18 = new("chr18", "18", "NC_000018.9", "", 78077248, 17);
    public static readonly Chromosome Chr19 = new("chr19", "19", "NC_000019.9", "", 59128983, 18);
    public static readonly Chromosome Chr20 = new("chr20", "20", "NC_000020.10", "", 63025520, 19);
    public static readonly Chromosome Chr21 = new("chr21", "21", "NC_000021.8", "", 48129895, 20);
    public static readonly Chromosome Chr22 = new("chr22", "22", "NC_000022.10", "", 51304566, 21);
    public static readonly Chromosome ChrX  = new("chrX", "X", "NC_000023.10", "", 155270560, 22);
    public static readonly Chromosome ChrY  = new("chrY", "Y", "NC_000024.9", "", 59373566, 23);
    public static readonly Chromosome ChrM  = new("chrM", "MT", "NC_012920.1", "", 16569, 24);

    public static readonly Chromosome Bob = new("bob", "bob", "", "", 1, Chromosome.UnknownReferenceIndex);

    public static readonly Dictionary<string, Chromosome> RefNameToChromosome = new();
    public static readonly Chromosome[]                   Chromosomes;

    static ChromosomeUtilities()
    {
        Chromosomes = new[]
        {
            Chr1, Chr2, Chr3, Chr4, Chr5, Chr6, Chr7, Chr8, Chr9, Chr10, Chr11, Chr12, Chr13, Chr14, Chr15, Chr16,
            Chr17, Chr18, Chr19, Chr20, Chr21, Chr22, ChrX, ChrY, ChrM
        };
        foreach (var chromosome in Chromosomes) AddChromosome(chromosome);
    }

    private static void AddChromosome(Chromosome chromosome)
    {
        RefNameToChromosome[chromosome.EnsemblName] = chromosome;
        RefNameToChromosome[chromosome.UcscName]    = chromosome;
    }
}