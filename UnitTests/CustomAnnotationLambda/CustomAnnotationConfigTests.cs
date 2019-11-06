using Cloud.Messages;
using Cloud.Messages.Custom;
using CustomAnnotationLambda;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.CustomAnnotationLambda
{
    public sealed class CustomAnnotationConfigTests
    {
        [Fact]
        public void CheckFieldsNotNull_AsExpected()
        {
            var config = GetConfig();
            config.id = null;
            var exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("id cannot be null.", exception.Message);

            config = GetConfig();
            config.tsvUrl = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("tsvUrl cannot be null.", exception.Message);

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

            config = GetConfig();
            config.outputDir.region = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("region of outputDir cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir.accessKey = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("accessKey of outputDir cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir.secretKey = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("secretKey of outputDir cannot be null.", exception.Message);

            config = GetConfig();
            config.outputDir.sessionToken = null;
            exception = Assert.Throws<UserErrorException>(() => config.CheckRequiredFieldsNotNull());
            Assert.Equal("sessionToken of outputDir cannot be null.", exception.Message);

        }

        private static CustomConfig GetConfig() => new CustomConfig
        {
            id = "Test",
            tsvUrl = "https://somewhere.org/input.tsv",
            outputDir = new S3Path
            {
                bucketName = "OutputBucket",
                path = "/OutputDir/",
                region = "nowhere",
                accessKey = "access",
                secretKey = "show me the money",
                sessionToken = "314159265"
            }
        };
    }
}