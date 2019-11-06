using System;
using Genome;

namespace SingleAnnotationLambda
{
    public sealed class CacheConfiguration : IEquatable<CacheConfiguration>
    {
        private readonly GenomeAssembly _genomeAssembly;
        private readonly string _supplementaryAnnotations;
        private readonly int _vepVersion;

        public CacheConfiguration(GenomeAssembly genomeAssembly, string supplementaryAnnotations, int vepVersion)
        {
            _genomeAssembly           = genomeAssembly;
            _supplementaryAnnotations = supplementaryAnnotations?.ToLower();
            _vepVersion               = vepVersion;
        }

        public bool Equals(CacheConfiguration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _genomeAssembly == other._genomeAssembly &&
                   string.Equals(_supplementaryAnnotations, other._supplementaryAnnotations) &&
                   _vepVersion == other._vepVersion;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) _genomeAssembly;
                if (_supplementaryAnnotations != null) hashCode = (hashCode * 397) ^ _supplementaryAnnotations.GetHashCode();
                hashCode = (hashCode * 397) ^ _vepVersion;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"genome assembly: {_genomeAssembly}, SA: {_supplementaryAnnotations}, VEP: {_vepVersion}";
        }
    }
}
