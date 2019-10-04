using System;
using Cloud;
using Genome;
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
    }
}
