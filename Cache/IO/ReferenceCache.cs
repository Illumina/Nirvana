using System;
using System.Linq;
using Genome;

namespace Cache.IO;

public sealed class ReferenceCache : IEquatable<ReferenceCache>
{
    public readonly Chromosome  Chromosome;
    public readonly CacheBin[] CacheBins;

    public ReferenceCache(Chromosome chromosome, CacheBin[] cacheBins)
    {
        Chromosome = chromosome;
        CacheBins  = cacheBins;
    }

    public bool Equals(ReferenceCache? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Chromosome.Equals(other.Chromosome) &&
            CacheBins.SequenceEqual(other.CacheBins);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Chromosome, CacheBins);
    }
}