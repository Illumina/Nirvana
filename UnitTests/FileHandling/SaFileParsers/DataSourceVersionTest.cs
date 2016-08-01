using System;
using System.IO;
using System.Text;
using SAUtils.InputFileParsers;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    public class DataSourceVersionTest
    {
        [Fact]
        public void ReadDataVersionFromFile()
        {
            DataSourceVersion version;
            using (var reader = new DataSourceVersionReader(Path.Combine("Resources", "dbSNP.version")))
            {
                version = reader.GetVersion();
            }

            Assert.Equal("dbSNP", version.Name);
            Assert.Equal("147", version.Version);
            Assert.Equal(DateTime.Parse("2016-04-08").Ticks, version.ReleaseDateTicks);
            Assert.True(string.IsNullOrEmpty(version.Description));
            Assert.Contains("dataSource=dbSNP", version.ToString());//vcf output

            var sb = new StringBuilder();
            version.SerializeJson(sb);

            Assert.Contains("name\":\"dbSNP", sb.ToString());//json output
        }
    }
}
