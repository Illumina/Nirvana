using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class BooleanExtensionsTests
    {
        [Theory]
        [InlineData("PASS", false)]
        [InlineData("LowGQX", true)]
        [InlineData(null, false)]
        public void GetFailedFilter(string filter, bool? expectedFailedFilter)
        {
            bool observedFailedFilter = filter.GetFailedFilter();
            Assert.Equal(expectedFailedFilter, observedFailedFilter);
        }
    }
}
