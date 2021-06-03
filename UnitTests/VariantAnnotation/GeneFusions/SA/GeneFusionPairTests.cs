using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.SA
{
    public sealed class GeneFusionPairTests
    {
        private readonly GeneFusionPair _fusionPair     = new(1000, "A", 123, "B", 456);
        private readonly GeneFusionPair _fusionPairDup  = new(1000, "A", 123, "B", 456);
        private readonly GeneFusionPair _fusionPairDiff = new(2000, "A", 123, "B", 456);

        [Fact]
        public void Equals_ExpectedResults()
        {
            Assert.False(_fusionPair.Equals(null));
            Assert.Equal(_fusionPair, _fusionPair);
            Assert.Equal(_fusionPair, _fusionPairDup);
            Assert.NotEqual(_fusionPair, _fusionPairDiff);
        }
        
        [Fact]
        public void Equals_IGeneFusionPair_ExpectedResults()
        {
            IGeneFusionPair fusionPair     = _fusionPair;
            IGeneFusionPair fusionPairDup  = _fusionPairDup;
            IGeneFusionPair fusionPairDiff = _fusionPairDiff;

            Assert.False(fusionPair.Equals(null));
            Assert.Equal(fusionPair, fusionPair);
            Assert.Equal(fusionPair, fusionPairDup);
            Assert.NotEqual(fusionPair, fusionPairDiff);
        }

        [Fact]
        public void GetHashCode_ExpectedResults()
        {
            Assert.Equal(_fusionPair.GetHashCode(), _fusionPairDup.GetHashCode());
            Assert.NotEqual(_fusionPair.GetHashCode(), _fusionPairDiff.GetHashCode());
        }
    }
}