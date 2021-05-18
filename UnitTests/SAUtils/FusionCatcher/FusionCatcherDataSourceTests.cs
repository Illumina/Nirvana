using System.Collections.Generic;
using System.IO;
using System.Text;
using SAUtils.FusionCatcher;
using VariantAnnotation.GeneFusions.SA;
using VariantAnnotation.GeneFusions.Utilities;
using Xunit;

namespace UnitTests.SAUtils.FusionCatcher
{
    public sealed class FusionCatcherDataSourceTests
    {
        [Fact]
        public void Parse_ExpectedResults()
        {
            var geneKeyToFusion = new Dictionary<ulong, GeneFusionSourceBuilder>();
            var knownEnsemblGenes = new HashSet<string>
            {
                "ENSG00000035499",
                "ENSG00000155959"
            };

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
            {
                writer.WriteLine("ENSG00000006210\tENSG00000102962");
                writer.WriteLine("ENSG00000006652\tENSG00000181016");
                writer.WriteLine("ENSG00000014138\tENSG00000149798");
                writer.WriteLine("ENSG00000026297\tENSG00000071242");
                writer.WriteLine("ENSG00000035499\tENSG00000155959");
                writer.WriteLine("ENSG00000055211\tENSG00000131013");
                writer.WriteLine("ENSG00000055332\tENSG00000179915");
                writer.WriteLine("ENSG00000062485\tENSG00000257727");
                writer.WriteLine("ENSG00000065978\tENSG00000166501");
                writer.WriteLine("ENSG00000066044\tENSG00000104980");
            }

            ms.Position = 0;

            FusionCatcherDataSource.Parse(ms, GeneFusionSource.OneK_Genomes_Project, CollectionType.Germline, geneKeyToFusion, knownEnsemblGenes);
            Assert.Single(geneKeyToFusion);

            ulong geneKey = GeneFusionKey.Create("ENSG00000035499", "ENSG00000155959");

            bool hasEntry = geneKeyToFusion.TryGetValue(geneKey, out GeneFusionSourceBuilder actualBuilder);
            Assert.True(hasEntry);
            Assert.Empty(actualBuilder.Relationships);
            Assert.Single(actualBuilder.GermlineSources);
            Assert.Empty(actualBuilder.SomaticSources);
            Assert.Equal(GeneFusionSource.OneK_Genomes_Project, actualBuilder.GermlineSources[0]);
        }

        [Fact]
        public void Parse_IncorrectFileFormat_ThrowException()
        {
            var geneKeyToFusion   = new Dictionary<ulong, GeneFusionSourceBuilder>();
            var knownEnsemblGenes = new HashSet<string>();

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
            {
                writer.WriteLine("ENSG00000006210\tENSG00000102962\tENSG00000181016");
            }

            ms.Position = 0;

            Assert.Throws<InvalidDataException>(delegate
            {
                FusionCatcherDataSource.Parse(ms, GeneFusionSource.OneK_Genomes_Project, CollectionType.Germline, geneKeyToFusion, knownEnsemblGenes);
            });
        }
        
        [Fact]
        public void Parse_MultipleCollections_ExpectedResults()
        {
            var geneKeyToFusion = new Dictionary<ulong, GeneFusionSourceBuilder>();
            var knownEnsemblGenes = new HashSet<string>
            {
                "ENSG00000035499",
                "ENSG00000155959"
            };

            using var ms = new MemoryStream();
            AddData(ms);
            FusionCatcherDataSource.Parse(ms, GeneFusionSource.Bao_gliomas, CollectionType.Somatic, geneKeyToFusion, knownEnsemblGenes);
            
            using var ms2 = new MemoryStream();
            AddData(ms2);
            FusionCatcherDataSource.Parse(ms2, GeneFusionSource.Readthrough, CollectionType.Relationships, geneKeyToFusion, knownEnsemblGenes);
            
            Assert.Single(geneKeyToFusion);

            ulong geneKey = GeneFusionKey.Create("ENSG00000035499", "ENSG00000155959");

            bool hasEntry = geneKeyToFusion.TryGetValue(geneKey, out GeneFusionSourceBuilder actualBuilder);
            Assert.True(hasEntry);
            Assert.Single(actualBuilder.Relationships);
            Assert.Empty(actualBuilder.GermlineSources);
            Assert.Single(actualBuilder.SomaticSources);
        }

        private static void AddData(MemoryStream ms)
        {
            using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
            {
                writer.WriteLine("ENSG00000035499\tENSG00000155959");
            }

            ms.Position = 0;
        }
    }
}