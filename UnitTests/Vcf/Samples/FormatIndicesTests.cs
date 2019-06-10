using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class FormatIndicesTests
    {
        [Fact]
        public void FormatIndicesTest()
        {
            const string formatColumn = "AD:AQ:BOB:CN:DN:DP:DST:FT:GQ:GT:LQ:PR:SR:VF";
            var formatIndices = new FormatIndices();
            formatIndices.Set(formatColumn);

            Assert.Equal(0, formatIndices.AD);
            Assert.Equal(1, formatIndices.AQ);
            Assert.Equal(3, formatIndices.CN);
            Assert.Equal(4, formatIndices.DN);
            Assert.Equal(5, formatIndices.DP);
            Assert.Equal(6, formatIndices.DST);
            Assert.Equal(7, formatIndices.FT);
            Assert.Equal(8, formatIndices.GQ);
            Assert.Equal(9, formatIndices.GT);
            Assert.Equal(10, formatIndices.LQ);
            Assert.Equal(11, formatIndices.PR);
            Assert.Equal(12, formatIndices.SR);
            Assert.Equal(13, formatIndices.VF);

            formatIndices.Set(null);
            Assert.False(formatIndices.AD.HasValue);
            Assert.False(formatIndices.AQ.HasValue);
            Assert.False(formatIndices.CN.HasValue);
            Assert.False(formatIndices.DN.HasValue);
            Assert.False(formatIndices.DP.HasValue);
            Assert.False(formatIndices.DST.HasValue);
            Assert.False(formatIndices.FT.HasValue);
            Assert.False(formatIndices.GQ.HasValue);
            Assert.False(formatIndices.GT.HasValue);
            Assert.False(formatIndices.LQ.HasValue);
            Assert.False(formatIndices.PR.HasValue);
            Assert.False(formatIndices.SR.HasValue);
            Assert.False(formatIndices.VF.HasValue);

            formatIndices.Set("TEMP:DP:BOB");
            Assert.Equal(1, formatIndices.DP);
        }
    }
}
