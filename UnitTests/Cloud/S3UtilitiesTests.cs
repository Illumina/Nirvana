using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Cloud;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class S3UtilitiesTests
    {
        [Fact]
        public void GetS3KeysFromEnvironment_AsExpect()
        {
            const string bucketName = "test-bucket";
            const string convertedBucketName = "test_bucket";
            const string accessKey = "ThisIsTheAccess";
            const string secretKey = "ThisIsTheSecret";
            const string regionEndpointString = "us-east-1";
            var regionEndpoint = RegionEndpoint.USEast1;

            Environment.SetEnvironmentVariable(convertedBucketName + "_access_key", accessKey);
            Environment.SetEnvironmentVariable(convertedBucketName + "_secret_key", secretKey);
            Environment.SetEnvironmentVariable(convertedBucketName + "_region_endpoint", regionEndpointString);

            var s3Keys = S3Utilities.GetS3KeysFromEnvironment(bucketName);

            Assert.Equal((accessKey, secretKey, regionEndpoint), s3Keys);
        }
    }
}
