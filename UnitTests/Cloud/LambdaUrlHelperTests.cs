using Cloud;
using Genome;
using IO;
using ReferenceSequence;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class LambdaUrlHelperTests
    {
        [Fact]
        public void GetDataUrlBase_AsExpected()
        {
            Assert.Equal($"http://somewhere.on.the.earth/ab0cf104f39708eabd07b8cb67e149ba-Cache/{CacheConstants.DataVersion}/", LambdaUrlHelper.GetCacheFolder("http://somewhere.on.the.earth/"));
            Assert.Equal($"http://somewhere.on.the.earth/d95867deadfe690e40f42068d6b59df8-References/{ReferenceSequenceCommon.HeaderVersion}/Homo_sapiens.", LambdaUrlHelper.GetRefPrefix("http://somewhere.on.the.earth/"));
        }

        [Fact]
        public void GetS3RefLocation_AsExpected()
        {
            Assert.Equal(LambdaUrlHelper.GetRefPrefix("whatever") + "GRCh37" + LambdaUrlHelper.RefSuffix, LambdaUrlHelper.GetRefUrl(GenomeAssembly.GRCh37, "whatever"));
        }
    }
}
