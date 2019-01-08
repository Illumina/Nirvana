using Amazon.S3;
using Amazon.S3.Transfer;

namespace Cloud
{
    public static class S3Utilities
    {
        public static void Upload(AmazonS3Client s3Client, string bucketName, string path, string localFilePath)
        {
            TransferUtility transferUtility = new TransferUtility(s3Client);

            TransferUtilityUploadRequest transferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                FilePath = localFilePath,
                Key = path.TrimStart('/')
            };

            transferUtility.Upload(transferUtilityRequest);
        }
    }
}