using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3.Model;

namespace Cloud
{
    public static class S3Utilities
    {
        private const string AccessKeySuffix = "_access_key";
        private const string SecretKeySuffix = "_secret_key";
        private const string RegionEndpointSuffix = "_region_endpoint";



        public static void DownloadS3Resource(S3Path input, string localFilePath)
        {
            using (var inputStream = new S3StreamSource(GetS3ClientWrapperFromEnvironment(input.bucketName), input).GetStream())
            using (var fileStream = File.Create(localFilePath))
            {
                inputStream.CopyTo(fileStream);
            }
        }

        public static string UploadBaseAndIndexFiles(IS3Client s3Client, S3Path targetS3DirPath, string localBaseFilePath, string s3BaseFileName, string indexSuffix)
        {

            s3Client.Upload(CombineS3DirAndFileName(targetS3DirPath, s3BaseFileName + indexSuffix), localBaseFilePath + indexSuffix);
            return s3Client.Upload(CombineS3DirAndFileName(targetS3DirPath, s3BaseFileName), localBaseFilePath);
        }

        public static S3Path CombineS3DirAndFileName(S3Path s3DirPath, string s3FileName) => new S3Path
        {
            bucketName = s3DirPath.bucketName,
            path = Path.Combine(s3DirPath.path, s3FileName),
        };

        internal static (string AccessKey, string SecretKey, RegionEndpoint RegionEndpoint) GetS3KeysFromEnvironment(
            string bucketName)
        {
            string accessKey = GetBucketInfo(bucketName, AccessKeySuffix);
            string secretKey = GetBucketInfo(bucketName, SecretKeySuffix);
            RegionEndpoint regionEndpoint =
                RegionEndpoint.GetBySystemName(GetBucketInfo(bucketName, RegionEndpointSuffix));
            return (accessKey, secretKey, regionEndpoint);
        }

        public static IS3Client GetS3ClientWrapperFromEnvironment(string bucketName)
        {
            var (accessKey, secretKey, regionEndpoint) = GetS3KeysFromEnvironment(bucketName);
            return new AmazonS3ClientWrapper(accessKey, secretKey, regionEndpoint);
        }

        private static string GetBucketInfo(string bucketName, string infoSuffix) =>
            Environment.GetEnvironmentVariable(GetTransformedBucketName(bucketName) + infoSuffix);

        //todo: temporary solution to handle characters that not valid for environmental variables
        private static string GetTransformedBucketName(string bucketNameWithDash)
        {
            Regex pattern = new Regex("-");
            return pattern.Replace(bucketNameWithDash, "_");
        }
    }
}