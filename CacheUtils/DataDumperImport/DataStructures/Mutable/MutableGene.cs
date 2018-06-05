using System;
using CacheUtils.Genes.DataStructures;
using Genome;
using Intervals;

namespace CacheUtils.DataDumperImport.DataStructures.Mutable
{
    public sealed class MutableGene : IEquatable<MutableGene>, IFlatGene<MutableGene>
    {
        public IChromosome Chromosome { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool OnReverseStrand { get; }
        public string GeneId { get; set; }
        public string Symbol { get; set; }
        public int HgncId { get; set; }
        public GeneSymbolSource SymbolSource { get; set; }

        public MutableGene(IChromosome chromosome, int start, int end, bool onReverseStrand, string symbol,
            GeneSymbolSource symbolSource, string geneId, int hgncId)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            OnReverseStrand = onReverseStrand;
            Symbol          = symbol;
            SymbolSource    = symbolSource;
            GeneId          = geneId;
            HgncId          = hgncId;
        }

        public override string ToString()
        {
            string strand = OnReverseStrand ? "R" : "F";
            return $"{GeneId}: {Chromosome.UcscName} {Start}-{End} {strand} symbol: {Symbol} ({SymbolSource}), HGNC ID: {HgncId}";
        }

        public bool Equals(MutableGene other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Chromosome.Index == other.Chromosome.Index &&
                   Start            == other.Start            &&
                   End              == other.End              &&
                   OnReverseStrand  == other.OnReverseStrand  &&
                   Symbol           == other.Symbol           &&
                   GeneId           == other.GeneId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                int hashCode = Chromosome.Index.GetHashCode();
                hashCode = (hashCode * 397) ^ Start;
                hashCode = (hashCode * 397) ^ End;
                hashCode = (hashCode * 397) ^ OnReverseStrand.GetHashCode();
                hashCode = (hashCode * 397) ^ Symbol.GetHashCode();
                hashCode = (hashCode * 397) ^ GeneId.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }

        public MutableGene Clone() => new MutableGene(Chromosome, Start, End, OnReverseStrand, Symbol, SymbolSource,
            GeneId, HgncId);

        public UgaGene ToUgaGene(bool isGrch37)
        {
            (string ensemblGeneId, string entrezGeneId) = GeneId.StartsWith("ENSG") ? (GeneId, null as string) : (null as string, GeneId);

            IInterval interval = new Interval(Start, End);
            (IInterval grch37, IInterval grch38) = isGrch37 ? (interval, null as IInterval) : (null as IInterval, interval);

            return new UgaGene(Chromosome, grch37, grch38, OnReverseStrand, entrezGeneId, ensemblGeneId, Symbol,
                HgncId);
        }
    }
}
