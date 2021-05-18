using VariantAnnotation.GeneFusions.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.SA
{
    public sealed class GeneFusionSourceCollectionTests
    {
        private readonly GeneFusionSourceCollection _sourceCollection =
            new(null, new[] {GeneFusionSource.Healthy}, new[] {GeneFusionSource.Bao_gliomas, GeneFusionSource.Robinson_prostate_cancers});

        private readonly GeneFusionSourceCollection _sourceCollectionDup =
            new(null, new[] {GeneFusionSource.Healthy}, new[] {GeneFusionSource.Bao_gliomas, GeneFusionSource.Robinson_prostate_cancers});

        private readonly GeneFusionSourceCollection _sourceCollectionDiff =
            new(new[] {GeneFusionSource.Paralog}, new[] {GeneFusionSource.Healthy}, new[]
                {GeneFusionSource.Bao_gliomas, GeneFusionSource.Robinson_prostate_cancers});
        
        [Fact]
        public void Equals_ExpectedResults()
        {
            Assert.False(_sourceCollection.Equals(null));
            Assert.Equal(_sourceCollection, _sourceCollection);
            Assert.Equal(_sourceCollection, _sourceCollectionDup);
            Assert.NotEqual(_sourceCollection, _sourceCollectionDiff);
        }

        [Fact]
        public void GetJsonEntry_ExpectedResults()
        {
            const string expectedJson = "\"genes\":[\"A\",\"B\"],\"germlineSources\":[\"Healthy\"],\"somaticSources\":[\"Bao gliomas\",\"Robinson prostate cancers\"]";
            string       actualJson   = _sourceCollection.GetJsonEntry(new[] {"A", "B"});
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void GetHashCode_ExpectedResults()
        {
            Assert.Equal(_sourceCollection.GetHashCode(), _sourceCollectionDup.GetHashCode());
            Assert.NotEqual(_sourceCollection.GetHashCode(), _sourceCollectionDiff.GetHashCode());
        }
    }
}