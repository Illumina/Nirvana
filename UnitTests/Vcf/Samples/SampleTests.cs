using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class SampleTests
    {
        [Fact]
        public void Sample_ReturnEmpty()
        {
            var emptySample = new Sample(null, null, null, null, false, null, null, false, null, null, null, null, null,
                null);

            Assert.True(emptySample.IsEmpty);
            Assert.True(Sample.EmptySample.IsEmpty);
        }
    }
}
