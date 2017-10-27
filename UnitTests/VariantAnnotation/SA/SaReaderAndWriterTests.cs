using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.SA
{
    public class SaReaderAndWriterTests
    {
        [Fact]
        public void SaReader_And_SaWriter_Tests()
        {
            var saMs = new MemoryStream();
            var indexMs = new MemoryStream();

            var dataSourceVersions = new[]
            {
                new DataSourceVersion("clinvar","20",DateTime.Today.Ticks,"clinvar dataset"),
                new DataSourceVersion("dbSnp","18",DateTime.Parse("12/20/2010").Ticks,"dbSNP") 
            };
            var header = new SupplementaryAnnotationHeader("chr1",DateTime.Now.Ticks,1,dataSourceVersions,GenomeAssembly.GRCh37);
            var smallIntervals = new List<ISupplementaryInterval>
            {
                new SupplementaryInterval("data1","chr1",100,150,"",ReportFor.SmallVariants)
            };
            var svIntervals = new List<ISupplementaryInterval>
            {
                new SupplementaryInterval("data2","chr1",100,1000,"",ReportFor.StructuralVariants)
            };

            var allIntervals = new List<ISupplementaryInterval>
            {
                new SupplementaryInterval("data3","chr1",100,1000,"",ReportFor.AllVariants)
            };

            var saDataSources = new ISaDataSource[4];
            saDataSources[0] = new SaDataSource("data1", "data1", "A", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[1] = new SaDataSource("data2", "data2", "T", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[2] = new SaDataSource("data3", "data3", "A", false, false, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[3] = new SaDataSource("data4", "data4", "T", false, false, "acd", new[] { "\"id\":\"123\"" });

            var saPos = new SaPosition(saDataSources, "A");

            using (var saWriter = new SaWriter(saMs, indexMs, header, smallIntervals, svIntervals, allIntervals, new List<(int, string)>(),true))
            {
                saWriter.Write(saPos, 150);
            }
            saMs.Position = 0;
            indexMs.Position = 0;
            ISaPosition obseveredPosition, obseveredPosition2;
            using (var saReader = new SaReader(saMs, indexMs))
            {
                 obseveredPosition = saReader.GetAnnotation(150);
                obseveredPosition2 = saReader.GetAnnotation(200);
            }

            Assert.Equal("A",obseveredPosition.GlobalMajorAllele);
            Assert.Equal(4, obseveredPosition.DataSources.Length);
            Assert.Null(obseveredPosition2);
        }
    }
}