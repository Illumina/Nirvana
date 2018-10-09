using OrchestrationLambda;
using Xunit;

namespace UnitTests.OrchestrationLambda
{
    public sealed class PartitionTests
    {
        [InlineData(100_000, false, PatitionStrategy.WholeVcf)]
        [InlineData(100_001, false, PatitionStrategy.ByChr)]
        [InlineData(1_000_001, false, PatitionStrategy.ByArm)]
        [InlineData(200_000, true, PatitionStrategy.WholeVcf)]
        [InlineData(200_001, true, PatitionStrategy.ByChr)]
        [InlineData(2_000_001, true, PatitionStrategy.ByArm)]
        [Theory]
        public void GetStrategy_AsExpected(long fileSize, bool isGvcf, PatitionStrategy expectedPartitionStrategy)
        {
            var strategy = Partition.GetStrategy(fileSize, isGvcf);
            Assert.Equal(strategy, expectedPartitionStrategy);
        }

    }
}
