using System;
using System.IO;
using System.Linq;
using Cloud;
using Genome;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class NirvanaHelperTests
    {
        [Fact]
        public void GetDataUrlBase_AsExpected()
        {
            Environment.SetEnvironmentVariable("NirvanaDataUrlBase", "http://somewhere.on.the.earth/");
            Assert.Equal("http://somewhere.on.the.earth/ab0cf104f39708eabd07b8cb67e149ba-Cache/26/", NirvanaHelper.S3CacheFolder);
            Assert.Equal("http://somewhere.on.the.earth/d95867deadfe690e40f42068d6b59df8-References/5/Homo_sapiens.", NirvanaHelper.S3RefPrefix);
        }

        [Fact]
        public void GetS3RefLocation_AsExpected()
        {
            Environment.SetEnvironmentVariable("NirvanaDataUrlBase", "whatever");
            Assert.Equal(NirvanaHelper.S3RefPrefix + "GRCh37" + NirvanaHelper.RefSuffix, NirvanaHelper.GetS3RefLocation(GenomeAssembly.GRCh37));
        }

        [Fact]
        public void CleanOutput_AsExpected()
        {
            var tempDir = RandomPath.GetRandomPath();
            Directory.CreateDirectory(tempDir);

            File.Create(Path.Combine(tempDir, "json1" + NirvanaHelper.JsonSuffix)).Close();
            File.Create(Path.Combine(tempDir, "json2" + NirvanaHelper.JsonSuffix)).Close();
            File.Create(Path.Combine(tempDir, "jsonIndex1" + NirvanaHelper.JsonIndexSuffix)).Close();
            File.Create(Path.Combine(tempDir, "jsonIndex2" + NirvanaHelper.JsonIndexSuffix)).Close();

            var otherFiles = new[] { "other1.31415926", "other2" }.Select(x => Path.Combine(tempDir, x)).ToArray();
            Array.Sort(otherFiles);
            File.Create(otherFiles[0]).Close();
            File.Create(otherFiles[1]).Close();

            NirvanaHelper.CleanOutput(tempDir);

            var remainFiles = Directory.GetFiles(tempDir).ToArray();
            Array.Sort(remainFiles);

            Assert.Equal(otherFiles.Length, remainFiles.Length);
            for (var i = 0; i < otherFiles.Length; i++)
            {
                Assert.Equal(otherFiles[i], remainFiles[i]);
            }
        }
    }
}
