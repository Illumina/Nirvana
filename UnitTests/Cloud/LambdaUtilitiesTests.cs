using Cloud.Utilities;
using ErrorHandling.Exceptions;
using Genome;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class LambdaUtilitiesTests
    {

        [Fact]
        public void ValidateCoreDataTest_no_exception()
        {
            //the test is that the following call doesn't throw an exception
            LambdaUtilities.ValidateCoreData(GenomeAssembly.GRCh37, "https://ilmn-nirvana.s3.us-west-2.amazonaws.com/");
        }

        [Fact]
        public void ValidateCoreDataTest_exception()
        {
            //the test is that the following call doesn't throw an exception
            Assert.Throws<DeploymentErrorException>(
                ()=>LambdaUtilities.ValidateCoreData(GenomeAssembly.GRCh37, "https://ilmn-nirvana-test.s3.us-west-2.amazonaws.com/"));
        }

        [Fact]
        public void ValidateSupplementaryDataTest_no_exception()
        {
            //the test is that the following call doesn't throw an exception
            LambdaUtilities.ValidateSupplementaryData(GenomeAssembly.GRCh37, "latest", "https://ilmn-nirvana.s3.us-west-2.amazonaws.com/");
        }

        [Fact]
        public void ValidateSupplementaryDataTest_exception()
        {
            //the test is that the following call doesn't throw an exception
            Assert.Throws<DeploymentErrorException>(
            ()=>LambdaUtilities.ValidateSupplementaryData(GenomeAssembly.GRCh37, "latest", "https://ilmn-nirvana-test.s3.us-west-2.amazonaws.com/"));
        }

    }
}
