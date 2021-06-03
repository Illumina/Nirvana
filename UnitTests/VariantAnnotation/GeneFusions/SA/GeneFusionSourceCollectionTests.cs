using VariantAnnotation.GeneFusions.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.SA
{
    public sealed class GeneFusionSourceCollectionTests
    {
        private readonly GeneFusionSourceCollection _sourceCollection =
            new(false, false, false, new[] {GeneFusionSource.Healthy}, new[]
                {GeneFusionSource.Bao_gliomas, GeneFusionSource.Robinson_prostate_cancers});

        private readonly GeneFusionSourceCollection _sourceCollectionDup =
            new(false, false, false, new[] {GeneFusionSource.Healthy}, new[]
                {GeneFusionSource.Bao_gliomas, GeneFusionSource.Robinson_prostate_cancers});

        private readonly GeneFusionSourceCollection _sourceCollectionDiff =
            new(false, true, false, new[] {GeneFusionSource.Healthy}, new[]
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
            const string expectedJson =
                "\"genes\":{\"first\":{\"hgnc\":\"A\"},\"second\":{\"hgnc\":\"B\"}},\"germlineSources\":[\"Healthy\"],\"somaticSources\":[\"Bao gliomas\",\"Robinson prostate cancers\"]";
            var    geneFusionPair = new GeneFusionPair(100, "A", 100, "B", 200);
            string actualJson     = _sourceCollection.GetJsonEntry(geneFusionPair, new uint[] {123});
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