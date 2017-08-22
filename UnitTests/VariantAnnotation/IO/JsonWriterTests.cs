using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.IO
{
    public sealed class JsonWriterTests
    {
        [Fact]
        public void WriteJsonEntry_Nominal()
        {
            var dataSourceVersions = new List<IDataSourceVersion> { new DataSourceVersion("nirvana", "2.0", 100) };
            var sampleNames = new[] { "NA12878" };

            string observedResult;

            using (var ms = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(ms, Encoding.ASCII, 1024, true))
                using (var writer       = new JsonWriter(streamWriter, "nirvana", "time", "vep", dataSourceVersions, "hg19", sampleNames))
                {
                    writer.WriteJsonEntry("{\"test\":\"good\"}");
                    writer.WriteJsonEntry("{\"crash\":\"bad\"}");
                    writer.WriteJsonEntry(null);
                }

                observedResult = Encoding.UTF8.GetString(ms.ToArray());
            }

            const string expectedResult = "{\"header\":{\"annotator\":\"nirvana\",\"creationTime\":\"time\",\"genomeAssembly\":\"hg19\",\"schemaVersion\":6,\"dataVersion\":\"vep\",\"dataSources\":[{\"name\":\"nirvana\",\"version\":\"2.0\",\"releaseDate\":\"0001-01-01\"}],\"samples\":[\"NA12878\"]},\"positions\":[\n{\"test\":\"good\"},\n{\"crash\":\"bad\"}\n]}\n";
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
