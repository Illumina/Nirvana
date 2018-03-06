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

        public override int GetHashCode(IGene x)
        {
            var entrezGeneId = x.EntrezGeneId.WithVersion;
            var ensemblId    = x.EnsemblId.WithVersion;

            unchecked
            {
                var hashCode = x.Start;
                hashCode = (hashCode * 397) ^ x.End;
                hashCode = (hashCode * 397) ^ x.Chromosome.Index;
                hashCode = (hashCode * 397) ^ x.OnReverseStrand.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Symbol.GetHashCode();
                if (entrezGeneId != null) hashCode = (hashCode * 397) ^ entrezGeneId.GetHashCode();
                if (ensemblId    != null) hashCode = (hashCode * 397) ^ ensemblId.GetHashCode();
                hashCode = (hashCode * 397) ^ x.HgncId;
                return hashCode;
            }
        }
    }
}
