using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;

namespace CacheUtils.Genes.DataStructures
{
    public sealed class UgaGene
    {
        public readonly IChromosome Chromosome;
        public readonly IInterval GRCh37;
        public readonly IInterval GRCh38;
        public readonly bool OnReverseStrand;
        public readonly int HgncId;

        public string Symbol { get; set; }
        public string EntrezGeneId { get; }
        public string EnsemblId { get; }

        public UgaGene(IChromosome chromosome, IInterval grch37, IInterval grch38, bool onReverseStrand,
            string entrezGeneId, string ensemblId, string symbol, int hgncId)
        {
            Chromosome       = chromosome;
            GRCh37           = grch37;
            GRCh38           = grch38;
            EntrezGeneId     = entrezGeneId;
            EnsemblId        = ensemblId;
            Symbol           = symbol;
            OnReverseStrand = onReverseStrand;
            HgncId           = hgncId;
        }

        public override string ToString()
        {
            string interval37 = GetInterval(GRCh37);
            string interval38 = GetInterval(GRCh38);
            string strand     = OnReverseStrand ? "R" : "F";
            return $"{Chromosome.UcscName}\t{Chromosome.EnsemblName}\t{Symbol}\t{interval37}\t{interval38}\t{strand}\t{HgncId}\t{EnsemblId}\t{EntrezGeneId}";
        }

        private static string GetInterval(IInterval interval) =>
            interval == null ? "-1\t-1" : $"{interval.Start}\t{interval.End}";

        public Gene ToGene(GenomeAssembly genomeAssembly)
        {
            var interval = genomeAssembly == GenomeAssembly.GRCh37 ? GRCh37 : GRCh38;
            return new Gene(Chromosome, interval.Start, interval.End, OnReverseStrand, Symbol, HgncId, CompactId.Convert(EntrezGeneId), CompactId.Convert(EnsemblId));
        }
    }
}
