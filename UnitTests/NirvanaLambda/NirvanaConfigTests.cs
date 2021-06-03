using Cloud.Messages;
using Cloud.Messages.Nirvana;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.NirvanaLambda
{
    public sealed class NirvanaConfigTests
    {
        [Fact]
        public void CheckFieldsNotNull_AsExpected()
        {
            var config = GetConfig();
            config.id = null;
            var exception = Assert.Throws<UserErrorException>(() =>config.CheckRequiredFieldsNotNull());
            Assert.Equal("id cannot be null.", exception.Message);

            config = GetConfig();
            config.genomeAssembly = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("genomeAssembly cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("outputDir cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir.bucketName = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("bucketName of outputDir cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir.path = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("path of outputDir cannot be null.", exception.Message);
            
        }

        private static NirvanaConfig GetConfig() => new NirvanaConfig
        {
            id = "Test",
            genomeAssembly = "Assembly",
            vcfUrl = "https://s3.amazonaws.com/input/input.vcf.gz?SomeStuff",
            tabixUrl = "https://s3.amazonaws.com/input/input.vcf.gz.tbi?SomeStuff",
            outputDir = new S3Path
            {
                bucketName = "OutputBucket",
                region = "us-west-2",
                path = "/OutputDir/",
                accessKey = "1234567",
                secretKey = "show me the money",
                sessionToken = "a token"
            }
        };
    }
}