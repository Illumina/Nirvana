using System;
using CacheUtils.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.CacheUtils.Utilities
{
    public sealed class RemoteFileTests
    {
        [Fact]
        public void GetFilename_WithoutUrlPrefix()
        {
            string expectedResult = $"ccds_1000_{Date.GetDate(DateTime.Now.Ticks)}.txt";
            var observedResult = RemoteFile.GetFilename("ccds_1000.txt", true);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetFilename_WithoutDate()
        {
            const string expectedResult = "CCDS2Sequence.20160908.txt";
            var observedResult = RemoteFile.GetFilename("ftp://ftp.ncbi.nlm.nih.gov/pub/CCDS/current_human/CCDS2Sequence.20160908.txt", false);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetFilename_WithUrlPrefix()
        {
            string expectedResult = $"CCDS2Sequence.20160908_{Date.GetDate(DateTime.Now.Ticks)}.txt";
            var observedResult = RemoteFile.GetFilename("ftp://ftp.ncbi.nlm.nih.gov/pub/CCDS/current_human/CCDS2Sequence.20160908.txt", true);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
