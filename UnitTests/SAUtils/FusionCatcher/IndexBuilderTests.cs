using System.Collections.Generic;
using SAUtils.FusionCatcher;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;
using Xunit;

namespace UnitTests.SAUtils.FusionCatcher
{
    public sealed class IndexBuilderTests
    {
        [Fact]
        public void Convert_ExpectedResults()
        {
            var expectedSourceCollection = new GeneFusionSourceCollection(false, true, false,
                new[] {GeneFusionSource.OneK_Genomes_Project, GeneFusionSource.Healthy}, new[] {GeneFusionSource.Alaei_Mahabadi_18_Cancers});

            var expectedSourceCollection2 = new GeneFusionSourceCollection(false, true, false, null, null);

            var expectedIndexEntries = new GeneFusionIndexEntry[]
            {
                new(1000, 0),
                new(2000, 0),
                new(3000, 0),
                new(4000, 1),
            };

            Dictionary<ulong, GeneFusionSourceBuilder> geneKeyToSourceBuilder = GetGeneKeyToSourceBuilder();

            (GeneFusionSourceCollection[] actualIndex, GeneFusionIndexEntry[] actualIndexEntries) = IndexBuilder.Convert(geneKeyToSourceBuilder);

            Assert.Equal(2,                         actualIndex.Length);
            Assert.Equal(expectedSourceCollection,  actualIndex[0]); // most common entry first
            Assert.Equal(expectedSourceCollection2, actualIndex[1]);

            Assert.Equal(4,                    actualIndexEntries.Length);
            Assert.Equal(expectedIndexEntries, actualIndexEntries);
        }

        private static Dictionary<ulong, GeneFusionSourceBuilder> GetGeneKeyToSourceBuilder()
        {
            var builder = new GeneFusionSourceBuilder
            {
                IsParalogPair = true,
                GermlineSources = {GeneFusionSource.OneK_Genomes_Project, GeneFusionSource.Healthy},
                SomaticSources  = {GeneFusionSource.Alaei_Mahabadi_18_Cancers}
            };
            
            var builder2 = new GeneFusionSourceBuilder
            {
                IsParalogPair   = true,
                GermlineSources = {GeneFusionSource.OneK_Genomes_Project, GeneFusionSource.Healthy},
                SomaticSources  = {GeneFusionSource.Alaei_Mahabadi_18_Cancers}
            };
            
            var builder3 = new GeneFusionSourceBuilder
            {
                IsParalogPair   = true,
                GermlineSources = {GeneFusionSource.OneK_Genomes_Project, GeneFusionSource.Healthy},
                SomaticSources  = {GeneFusionSource.Alaei_Mahabadi_18_Cancers}
            };
            
            var builder4 = new GeneFusionSourceBuilder
            {
                IsParalogPair = true
            };

            return new Dictionary<ulong, GeneFusionSourceBuilder>
            {
                [1000] = builder,
                [2000] = builder2,
                [3000] = builder3,
                [4000] = builder4
            };
        }
    }
}