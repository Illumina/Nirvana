using System.Collections.Generic;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.Utilities
{
    public sealed class GeneComparerTests
    {
        private readonly IGene _geneA;
        private readonly IGene _geneB;
        private readonly IGene _geneC;
        private readonly GeneComparer _geneComparer;

        public GeneComparerTests()
        {
            _geneA         = new Gene(ChromosomeUtilities.Chr1, 100, 200, false, "PAX", 123, CompactId.Convert("NM_123"), CompactId.Convert("ENST0000123"));
            _geneB         = new Gene(ChromosomeUtilities.Chr1, 100, 200, false, "PAX", 123, CompactId.Convert("NM_123"), CompactId.Convert("ENST0000123"));
            _geneC         = new Gene(ChromosomeUtilities.Chr1, 101, 200, false, "PAX", 123, CompactId.Convert("NM_123"), CompactId.Convert("ENST0000123"));
            _geneComparer  = new GeneComparer();
        }

        [Fact]
        public void Equals_AsExpected()
        {
            Assert.Equal(_geneA, _geneB, _geneComparer);
            Assert.NotEqual(_geneA, _geneC, _geneComparer);
        }

        [Fact]
        public void GetHashCode_AsExpected()
        {
            IGene geneD = new Gene(_geneA.Chromosome, 100, 200, false, "PAX", 123, CompactId.Convert("NM_123", 2), CompactId.Convert("ENST0000123"));

            var hashCodes = new HashSet<int>
            {
                _geneComparer.GetHashCode(_geneA),
                _geneComparer.GetHashCode(_geneB),
                _geneComparer.GetHashCode(_geneC),
                _geneComparer.GetHashCode(geneD)
            };

            Assert.Equal(3, hashCodes.Count);
        }
    }
}
