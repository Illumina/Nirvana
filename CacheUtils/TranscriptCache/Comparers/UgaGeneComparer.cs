using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.TranscriptCache.Comparers
{
    public sealed class UgaGeneComparer : EqualityComparer<UgaGene>
    {
        public override bool Equals(UgaGene x, UgaGene y)
        {
            if (ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.Chromosome.Index == y.Chromosome.Index &&
                Equals(x.GRCh37, y.GRCh37)                  &&
                Equals(x.GRCh38, y.GRCh38)                  &&
                x.GRCh38              == y.GRCh38           &&
                x.OnReverseStrand     == y.OnReverseStrand  &&
                x.HgncId              == y.HgncId           &&
                x.Symbol              == y.Symbol           &&
                x.EntrezGeneId        == y.EntrezGeneId     &&
                x.EnsemblId           == y.EnsemblId;
        }

        private static bool Equals(IInterval x, IInterval y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Start == y.Start && x.End == y.End;
        }

        private static int GetHashCode(IInterval x)
        {
            unchecked { return (x.Start * 397) ^ x.End; }
        }


        public override int GetHashCode(UgaGene obj)
        {
            unchecked
            {
                int hashCode = obj.Chromosome.Index.GetHashCode();
                if (obj.GRCh37 != null) hashCode = (hashCode * 397) ^ GetHashCode(obj.GRCh37);
                if (obj.GRCh38 != null) hashCode = (hashCode * 397) ^ GetHashCode(obj.GRCh38);
                hashCode = (hashCode * 397) ^ obj.OnReverseStrand.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.HgncId;
                if (obj.Symbol != null) hashCode = (hashCode * 397) ^ obj.Symbol.GetHashCode();
                if (obj.EntrezGeneId != null) hashCode = (hashCode * 397) ^ obj.EntrezGeneId.GetHashCode();
                if (obj.EnsemblId != null) hashCode = (hashCode * 397) ^ obj.EnsemblId.GetHashCode();
                return hashCode;
            }
        }
    }
}
