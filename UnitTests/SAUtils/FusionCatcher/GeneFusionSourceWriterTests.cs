using System;
using System.IO;
using Genome;
using SAUtils.FusionCatcher;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.SAUtils.FusionCatcher
{
    public sealed class GeneFusionSourceWriterTests
    {
        [Fact]
        public void GeneFusionSourceWriter_ExpectedResults()
        {
            (uint[] expectedOncogeneKeys, GeneFusionSourceCollection[] expectedIndex, GeneFusionIndexEntry[] expectedIndexEntries) =
                GetKeyToGeneFusion();

            IDataSourceVersion expectedVersion = new DataSourceVersion("FusionCatcher", "1.33", DateTime.Now.Ticks, "gene fusions");
            const string       expectedJsonKey = "fusionCatcher";

            using var ms = new MemoryStream();
            using (var writer = new GeneFusionSourceWriter(ms, expectedJsonKey, expectedVersion, true))
            {
                writer.Write(expectedOncogeneKeys, expectedIndex, expectedIndexEntries);
            }

            ms.Position = 0;

            uint[]                       actualOncogeneKeys;
            GeneFusionSourceCollection[] actualIndex;
            GeneFusionIndexEntry[]       actualIndexEntries;
            IDataSourceVersion           actualVersion;
            string                       actualJsonKey;
            GenomeAssembly               actualAssembly;

            using (var reader = new GeneFusionSourceReader(ms))
            {
                reader.LoadAnnotations();
                actualOncogeneKeys = reader.OncogeneKeys;
                actualIndex        = reader.Index;
                actualIndexEntries = reader.IndexEntries;
                actualVersion      = reader.Version;
                actualJsonKey      = reader.JsonKey;
                actualAssembly     = reader.Assembly;
            }

            Assert.Equal(expectedVersion,             actualVersion, new DataSourceVersionComparer());
            Assert.Equal(expectedJsonKey,             actualJsonKey);
            Assert.Equal(expectedOncogeneKeys,        actualOncogeneKeys);
            Assert.Equal(expectedIndex.Length,        actualIndex.Length);
            Assert.Equal(expectedIndex,               actualIndex);
            Assert.Equal(expectedIndexEntries.Length, actualIndexEntries.Length);
            Assert.Equal(expectedIndexEntries,        actualIndexEntries);
            Assert.Equal(GenomeAssembly.Unknown,      actualAssembly);
        }

        internal static (uint[] OncogeneKeys, GeneFusionSourceCollection[] Index, GeneFusionIndexEntry[] IndexEntries) GetKeyToGeneFusion()
        {
            uint[] oncogeneKeys = {123};
            var    index        = new GeneFusionSourceCollection[3];

            var fusionsWithBothSources = new GeneFusionSourceBuilder {IsParalogPair = true};
            fusionsWithBothSources.GermlineSources.Add(GeneFusionSource.OneK_Genomes_Project);
            fusionsWithBothSources.GermlineSources.Add(GeneFusionSource.Healthy_strong_support);
            fusionsWithBothSources.GermlineSources.Add(GeneFusionSource.Illumina_BodyMap2);
            fusionsWithBothSources.SomaticSources.Add(GeneFusionSource.Alaei_Mahabadi_18_Cancers);
            fusionsWithBothSources.SomaticSources.Add(GeneFusionSource.CCLE);
            index[0] = fusionsWithBothSources.Create();

            var germlineFusions = new GeneFusionSourceBuilder {IsPseudogenePair = true, IsReadthrough = true};
            germlineFusions.GermlineSources.Add(GeneFusionSource.CACG);
            germlineFusions.GermlineSources.Add(GeneFusionSource.ConjoinG);
            germlineFusions.GermlineSources.Add(GeneFusionSource.Healthy_prefrontal_cortex);
            germlineFusions.GermlineSources.Add(GeneFusionSource.Duplicated_Genes_Database);
            index[1] = germlineFusions.Create();

            var somaticFusions = new GeneFusionSourceBuilder();
            somaticFusions.SomaticSources.Add(GeneFusionSource.CCLE_Vellichirammal);
            somaticFusions.SomaticSources.Add(GeneFusionSource.Cancer_Genome_Project);
            index[2] = somaticFusions.Create();

            var indexEntries = new GeneFusionIndexEntry[]
            {
                new(1000, 0),
                new(2000, 1),
                new(3000, 2)
            };

            return (oncogeneKeys, index, indexEntries);
        }
    }
}