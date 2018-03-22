using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache.Comparers
{
    internal sealed class GeneComparer : EqualityComparer<IGene>
    {
        public override bool Equals(IGene x, IGene y)
        {
            return x.Start                    == y.Start                    &&
                   x.End                      == y.End                      &&
                   x.Chromosome.Index         == y.Chromosome.Index         &&
                   x.OnReverseStrand          == y.OnReverseStrand          &&
                   x.Symbol                   == y.Symbol                   &&
                   x.EntrezGeneId.WithVersion == y.EntrezGeneId.WithVersion &&
                   x.EnsemblId.WithVersion    == y.EnsemblId.WithVersion    &&
                   x.HgncId                   == y.HgncId;
        }

        public override int GetHashCode(IGene obj)
        {
            string entrezGeneId = obj.EntrezGeneId.WithVersion;
            string ensemblId    = obj.EnsemblId.WithVersion;

            unchecked
            {
                int hashCode = obj.Start;
                hashCode = (hashCode * 397) ^ obj.End;
                hashCode = (hashCode * 397) ^ obj.Chromosome.Index;
                hashCode = (hashCode * 397) ^ obj.OnReverseStrand.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Symbol.GetHashCode();
                if (entrezGeneId != null) hashCode = (hashCode * 397) ^ entrezGeneId.GetHashCode();
                if (ensemblId    != null) hashCode = (hashCode * 397) ^ ensemblId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.HgncId;
                return hashCode;
            }
        }
    }
}
