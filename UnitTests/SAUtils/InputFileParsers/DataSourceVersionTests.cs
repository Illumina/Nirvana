using System;
using System.Text;
using SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.SaUtilsTests.InputFileParsers
{
	public class DataSourceVersionTests
	{
		[Fact]
		public void ReadDataVersionFromFile()
		{
		    var versionFile = ResourceUtilities.GetReadStream(Resources.TopPath("dbSNP.version"));

		    DataSourceVersion version;
		    using (var reader = new DataSourceVersionReader(versionFile))
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

		[Fact]
		public void GetSourceVersionTest()
		{
			var versionPath = Resources.TopPath("dbSNP.version");

			var version = DataSourceVersionReader.GetSourceVersion(versionPath);

			Assert.Equal("dbSNP", version.Name);
			Assert.Equal("147", version.Version);
			Assert.Equal(DateTime.Parse("2016-04-08").Ticks, version.ReleaseDateTicks);
		}
	}
}
