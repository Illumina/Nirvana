using OrchestrationLambda;
using Xunit;

namespace UnitTests.OrchestrationLambda
{
    public sealed class OrchestratorTests
    {
        [Theory]
        [InlineData("/tmp/ada.vcf", 0, "ada_00000")]
        [InlineData("/ada.vcf", 1, "ada_00001")]
        [InlineData("ada.vcf", 2, "ada_00002")]
        [InlineData("ada.vcf.gz", 3, "ada_00003")]
        [InlineData("ada.vcf.data.vcf.gz", 4, "ada_00004")]
        public void GetIndexedPrefix_AsExpected(string inputVcfPath, int jobIndex, string expectedPrefix)
        {
            Assert.Equal(Orchestrator.GetIndexedPrefix(inputVcfPath, jobIndex), expectedPrefix);
        }
    }
}
