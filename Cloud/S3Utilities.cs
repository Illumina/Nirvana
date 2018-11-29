using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ErrorHandling.Exceptions;

namespace Cloud
{
    public static class S3Utilities
    {
        private const string AccessKeySuffix = "_access_key";
        private const string SecretKeySuffix = "_secret_key";
        private const string RegionEndpointSuffix = "_region_endpoint";


        public static S3Path CombineS3DirAndFileName(S3Path s3DirPath, string s3FileName) =>
            new S3Path()
            {
                bucketName = s3DirPath.bucketName,
                path = Path.Combine(s3DirPath.path, s3FileName)
            };

        public static (string AccessKey, string SecretKey, RegionEndpoint RegionEndpoint) GetS3KeysFromEnvironment(
            string bucketName)
        {
            string accessKey = GetBucketInfo(bucketName, AccessKeySuffix);
            string secretKey = GetBucketInfo(bucketName, SecretKeySuffix);
            RegionEndpoint regionEndpoint =
                RegionEndpoint.GetBySystemName(GetBucketInfo(bucketName, RegionEndpointSuffix));

            return (accessKey, secretKey, regionEndpoint);
        }

        public static AmazonS3Client GetAmazonS3Client(string bucketName)
        {
            var (accessKey, secretKey, regionEndpoint) = GetS3KeysFromEnvironment(bucketName);
            return new AmazonS3Client(accessKey, secretKey, regionEndpoint);
        }

        private static string GetBucketInfo(string bucketName, string infoSuffix)
        {

            string info = Environment.GetEnvironmentVariable(GetTransformedBucketName(bucketName) + infoSuffix);
            if (info == null) throw new UnauthorizedAccessException($"S3 bucket \"{bucketName}\" is not supported by the Lambda instance. Please use a supported S3 bucket.");
            return info;
        }

        //todo: temporary solution to handle characters that not valid for environmental variables
        private static string GetTransformedBucketName(string bucketNameWithDash)
        {
            Regex pattern = new Regex("-");
            return pattern.Replace(bucketNameWithDash, "_");
        }

        public static void Upload(AmazonS3Client s3Client, string bucketName, string path, string localFilePath)
        {
            TransferUtility transferUtility = new TransferUtility(s3Client);

            TransferUtilityUploadRequest transferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                FilePath = localFilePath,
                Key = path
            };

            transferUtility.Upload(transferUtilityRequest);
        }

        public static bool FileExist(AmazonS3Client client, S3Path s3Path)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = s3Path.bucketName,
                Key = s3Path.path.TrimStart('/'),
                Expires = DateTime.Now.AddMinutes(1)
            };

            return CheckUrlExist(client.GetPreSignedURL(request));
        }

        private static bool CheckUrlExist(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                webRequest.GetResponse();
            }
            catch //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }
    }
}