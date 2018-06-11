using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace Phantom.Utilities
{
    internal sealed class GeneComparer : EqualityComparer<IGene>
    {
        public override bool Equals(IGene x, IGene y)
        {
            return x.Start == y.Start &&
                   x.End == y.End &&
                   x.Chromosome.Index == y.Chromosome.Index &&
                   x.OnReverseStrand == y.OnReverseStrand &&
                   x.Symbol == y.Symbol &&
                   x.EntrezGeneId.WithVersion == y.EntrezGeneId.WithVersion &&
                   x.EnsemblId.WithVersion == y.EnsemblId.WithVersion &&
                   x.HgncId == y.HgncId;
        }

        public override int GetHashCode(IGene obj)
        {
            var entrezGeneId = obj.EntrezGeneId.WithVersion;
            var ensemblId = obj.EnsemblId.WithVersion;

            unchecked
            {
                var hashCode = obj.Start;
                hashCode = (hashCode * 397) ^ obj.End;
                hashCode = (hashCode * 397) ^ obj.Chromosome.Index;
                hashCode = (hashCode * 397) ^ obj.OnReverseStrand.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Symbol.GetHashCode();
                if (entrezGeneId != null) hashCode = (hashCode * 397) ^ entrezGeneId.GetHashCode();
                if (ensemblId != null) hashCode = (hashCode * 397) ^ ensemblId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.HgncId;
                return hashCode;
            }
        }
    }

    internal sealed class ListComparer<T> : IEqualityComparer<List<T>>
    {
        public bool Equals(List<T> x, List<T> y)
        {
            return x.SequenceEqual(y);
        }
        public int GetHashCode(List<T> obj)
        {
            int hashCode = 0;
            foreach (T t in obj)
            {
                if (t != null) hashCode = (hashCode * 397) ^ t.GetHashCode();
            }
            return hashCode;
        }
    }

    internal sealed class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            return x.SequenceEqual(y);
        }
        public int GetHashCode(T[] obj)
        {
            int hashCode = 0;
            foreach (T t in obj)
            {
                if (t != null) hashCode = (hashCode * 397) ^ t.GetHashCode();
            }
            return hashCode;
        }
    }

}